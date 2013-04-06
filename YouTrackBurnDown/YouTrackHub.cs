using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using YouTrackBurnDown.Api;

namespace YouTrackBurnDown
{
    public class YouTrackHub : Hub
    {

        public Tuple<string, string, DateTime, DateTime> SprintDates(string projectShortName, string sprint)
        {
            return new Client().GetSprintDates(projectShortName, sprint);
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, object>>> Query(
            string[] filters,
            string[] fields,
            int pageSize,
            int pageNumber)
        {
            var queryResult = new Client().QueryIssues(filters, fields, pageSize, pageNumber);

            var multiSearchResult = queryResult as MultiSearchResult;
            var singleSearchResult = queryResult as SearchResult;

            if (multiSearchResult != null)
            {
                var result = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                foreach (var data in multiSearchResult.SearchResult)
                    result[data.Search] = data.Issues.Select(Parse).ToDictionary(kv => kv.Key, kv => kv.Value);
                return result;
            }

            if (singleSearchResult != null)
            {
                var result = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                result[filters.First()] = singleSearchResult.Issue.Select(Parse).ToDictionary(kv => kv.Key, kv => kv.Value);
                return result;
            }

            return null;
        }

        private static DateTime ParseFromMilliseconds(long milliseconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(milliseconds);
        }

        private static KeyValuePair<string, Dictionary<string, object>> Parse(Issue issue)
        {
            var fieldMap = new Dictionary<string, object>();
            foreach (var field in issue.Field)
            {
                switch (field.Name)
                {
                    case "resolved":
                        fieldMap["resolved"] = ParseFromMilliseconds(long.Parse(field.Value));
                        break;
                    default:
                        fieldMap[field.Name] = field.Value;
                        break;
                }
            }
            return new KeyValuePair<string, Dictionary<string, object>>(issue.Id, fieldMap);
        }

    }
}