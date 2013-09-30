using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BetterBurnDown.YouTrack
{
    public class DavidWeekleyConventions : IBurnDownSystem
    {

        private readonly Client _client = new Client();


        public IEnumerable<ISprint> GetSprints()
        {
            var projects = _client.GetProjects();

            var items = projects
                .SelectMany(p => p.Sprints, (p, s) => Tuple.Create(p.Name, s));

            return GetSprints(items);
        }

        private ISprint GetSprint(string sprintId)
        {
            var projects = _client.GetProjects();

            var items = projects
                .SelectMany(p => p.Sprints, (p, s) => Tuple.Create(p.Name, s))
                .Where(t => t.Item2 == sprintId);

            var sprints = GetSprints(items).ToArray();

            return new Sprint()
                {
                    Id = sprintId,
                    Start = sprints.Min(s => s.Start),
                    End = sprints.Max(s => s.End)
                };
        }

        private IEnumerable<ISprint> GetSprints(IEnumerable<Tuple<string, string>> projectSprintIds)
        {
            var sprints = new Dictionary<string, Sprint>();

            var items = projectSprintIds
                .Select(t => new {ProjectName = t.Item1, SprintId = t.Item2});

            foreach (var item in items)
            {
                var result = _client.GetSprintDates(item.ProjectName, item.SprintId);
                result.Start = result.Start.Date;
                result.End = result.End.Date;

                if (!sprints.ContainsKey(item.SprintId))
                {
                    sprints[item.SprintId] = result;
                }
                else
                {
                    var sprint = sprints[item.SprintId];
                    if (result.Start < sprint.Start) sprint.Start = result.Start;
                    if (result.End > sprint.End) sprint.End = result.End;
                }
            }
            return sprints.Values;
        }

        public IGraph GetGraph(string sprintId)
        {
            var sprintFilter = " Fix versions: " + sprintId;
            var lineDefinitions = new Dictionary<string, string>
                {
                    {"#{Business Systems} -land-legal -JDEdwards -Construction -Vendor" + sprintFilter, "BS.NET"},
                    {"#{Business Systems} #{JDEdwards}" + sprintFilter, "JDE"},
                    {"#{Marketing Systems}" + sprintFilter, "Marketing"},
                    {"#{land-legal} #{Construction} #{Vendor}" + sprintFilter, "Headspring"}
                };

            var fields = new[] {"id", "resolved", "Estimation", "summary", "type"};

            var ignoreTypes = new[] {"Feature", "Epic", "User Story"};

            var sprint = GetSprint(sprintId);

            var results = (MultiSearchResult) _client.QueryIssues(lineDefinitions.Keys.ToArray(), fields, 2000, 1);

            var issues = results.SearchResult
                                .SelectMany(sr => sr.Issues,
                                            (sr, issue) => new Issue(lineDefinitions[sr.Search], issue))
                                .ToArray();

            var graph = new Graph();

            var ideal = graph.GenerateIdealLine(sprint.Start, sprint.End);

            foreach (var group in issues.GroupBy(i => i.LineLabel))
            {
                var line = graph.AddLine(group.Key);
                FillLine(line, sprint, group.ToArray(), ignoreTypes);
            }

            ideal = graph.Lines.First();

            if (!ideal.Begin.HasValue || !ideal.End.HasValue)
                return graph.Normalize(); 

            if (DateTime.UtcNow < ideal.Begin.Value || DateTime.UtcNow > ideal.End.Value)
                return graph.Normalize();

            foreach (var line in graph.Lines.Except(new[] {ideal}).Cast<Line>())
            {
                if (!line.Points.Any())
                    continue;

                if (line.End.HasValue && line.End.Value > ideal.End.Value)
                    continue;

                if (line.Min.HasValue && line.Min.Value < 0.01)
                    continue;

                // Figure out how far to project this line...
                var lastTS = DateTime.UtcNow;
                if (lastTS > ideal.End.Value)
                    lastTS = ideal.End.Value;
                if (line.End.Value > lastTS)
                    lastTS = line.End.Value;
                var lastValue = line.Points.Last().Value;

                // Project 0 progress through this moment
                var point = line.AddPoint(lastValue, lastTS);
                point.IsProjection = true;
                point.OriginalValue = (float) Math.Round(lastValue);

                if (lastTS < ideal.End.Value)
                {
                    // Project average progress through the end of the sprint
                    var elapsed = line.End.Value.Subtract(line.Begin.Value).Ticks;
                    var completed = line.Min.Value - line.Max.Value;

                    if (Math.Abs(completed - 0) < 0.001)
                        continue;

                    var rate = completed/elapsed;
                    
                    // y = mx + b;
                    // 0 = rate * x + max;
                    // -max = rate * x;
                    // -max / rate = x;
                    var estimatedCompletion = line.Begin.Value.AddTicks(Convert.ToInt64(-line.Max.Value/rate));

                    // y = mx + b
                    // y = rate * sprintTicks + max
                    var sprintTicks = ideal.End.Value.Subtract(line.Begin.Value).Ticks;

                    var estimatedPointsRemaining = rate*sprintTicks + line.Max.Value;

                    point = estimatedCompletion > ideal.End.Value
                                ? line.AddPoint(estimatedPointsRemaining, ideal.End.Value)
                                : line.AddPoint(0, estimatedCompletion);

                    point.OriginalValue = (float) Math.Round(point.Value);
                    
                    point.Label = "Points remaining at sprint completion: " + Convert.ToInt64(estimatedPointsRemaining);
                    point.Description = "Estimated completion of points: " + estimatedCompletion.ToString("f");

                    point.Label = string.Format("{0} by {1} points",
                                                estimatedPointsRemaining >= 0 ? "Over" : "Under",
                                                Convert.ToInt64(Math.Abs(estimatedPointsRemaining)));
                    point.IsProjection = true;
                }
            }
            return graph.Normalize();
        }

        private void FillLine(Line line, ISprint sprint, Issue[] issues, IEnumerable<string> ignoreTypes)
        {
            var sprintStart = new DateTime(sprint.Start.Ticks, DateTimeKind.Local);

            issues = issues.Where(i => !ignoreTypes.Contains(i.Type)).ToArray();
            var issueCount = issues.Count(i => i.Value.HasValue);
            var issueTotal = issues.Sum(i => i.Value != null ? i.Value.Value : 0);
            
            var averageValue = issueTotal/issueCount;

            issueTotal = issues.Sum(i => i.Value != null ? i.Value.Value : averageValue);

            line.AddPoint(issueTotal, sprintStart);

            var pointIssues = issues.Where(i => i.Timestamp.HasValue).OrderBy(i => i.Timestamp.Value);
            foreach (var pointIssue in pointIssues)
            {
                var value = pointIssue.Value.HasValue ? pointIssue.Value.Value : averageValue;
                var change = (float) (value*-1.0);
                var timestamp = pointIssue.Timestamp.Value;
                if (timestamp < sprintStart)
                    timestamp = sprintStart;
                var point = line.AddPointByChange(change, timestamp);
                point.Label = pointIssue.Id;
                point.Description = pointIssue.Summary;
                point.Estimate = pointIssue.Value.HasValue ? (int) pointIssue.Value.Value : (int?) null;
                point.Url = "http://davidweekleyhomes.myjetbrains.com/youtrack/issue/" + pointIssue.Id;
            }
        }

        private class Issue
        {

            public Issue(string lineLabel, YouTrack.Issue issue)
            {
                if (lineLabel == null) throw new ArgumentNullException("lineLabel");
                if (issue == null) throw new ArgumentNullException("issue");
                LineLabel = lineLabel;
                Id = issue.Id;
                Summary = GetFieldValue(issue, "summary");
                Value = GetValue(issue);
                Timestamp = GetTimeStamp(issue);

                var typeData = GetFieldValue(issue, "Type");
                if (!string.IsNullOrWhiteSpace(typeData))
                {
                    var type = JsonConvert.DeserializeObject<string[]>(typeData);
                    Type = type.First();
                }
            }

            private string GetFieldValue(YouTrack.Issue issue, string fieldName)
            {
                var item = issue.Field.SingleOrDefault(i => string.Equals(i.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));
                return item != null ? item.Value : null;
            }

            private float? GetValue(YouTrack.Issue issue)
            {
                var stringValue = GetFieldValue(issue, "Estimation");
                if (string.IsNullOrWhiteSpace(stringValue)) return null;
                var values = JsonConvert.DeserializeObject<int[]>(stringValue);
                if (values.Any()) return values.First();
                return null;
            }

            private DateTime? GetTimeStamp(YouTrack.Issue issue)
            {
                var stringValue = GetFieldValue(issue, "resolved");
                if (string.IsNullOrWhiteSpace(stringValue)) return null;
                return Client.Convert(Convert.ToInt64(stringValue));
            }

            public string LineLabel { get; set; }
            public string Id { get; set; }
            public string Summary { get; set; }
            public float? Value { get; set; }
            public DateTime? Timestamp { get; set; }
            public string Type { get; set; }

        }
    }
}
