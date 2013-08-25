using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBurnDown
{
    public static class GraphIdealLineExtensions
    {

        private class Interval
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public float StaffingPercentage { get; set; }
            public TimeSpan ActualTimespan
            {
                get { return End.Subtract(Start); }
            }

            public float PersonMinutes
            {
                get { return (float) ActualTimespan.TotalMinutes*StaffingPercentage; }
            }
        }

        public static void GenerateIdealLine(this Graph graph, DateTime firstDay, DateTime lastDay)
        {
            var staffingPercentages = GenerateTypicalStaffingPercentages(firstDay, lastDay).ToArray();
            graph.GenerateIdealLine(staffingPercentages.First().Key, staffingPercentages.Skip(1));
        }

        public static void GenerateIdealLine(this Graph graph, DateTime start,
                                             IEnumerable<KeyValuePair<DateTime, float>> staffingPercentages)
        {
            staffingPercentages = staffingPercentages.OrderBy(kv => kv.Key);

            var intervals = GenerateIntervals(start, staffingPercentages.ToArray()).ToArray();
            var totalPersonMinutes = intervals.Sum(i => i.PersonMinutes);

            var remainingPercentage = 1.0;
            var percentages = intervals
                .Select(i => new {i.Start, i.End, Percentage = i.PersonMinutes/totalPersonMinutes})
                .Select(x =>
                    {
                        remainingPercentage -= x.Percentage;
                        if (Math.Abs(remainingPercentage) < 0.0001) remainingPercentage = 0.0;
                        return new {x.Start, x.End, Percentage = (float) remainingPercentage};
                    });

            var ideal = graph.AddLine("Ideal");
            ideal.AddPoint((float) 1.0, start);
            percentages.ToList().ForEach(x => ideal.AddPoint(x.Percentage, x.End));
        }

        private static IEnumerable<Interval> GenerateIntervals(DateTime start, KeyValuePair<DateTime, float>[] staffingPercentages)
        {
            if (!staffingPercentages.Any())
                throw new ApplicationException("Staffing percentages required");

            var lastEnd = start;
            foreach (var workforcePercentage in staffingPercentages)
            {
                var interval = new Interval()
                    {
                        Start = lastEnd,
                        End = workforcePercentage.Key,
                        StaffingPercentage = workforcePercentage.Value
                    };
                lastEnd = interval.End;
                yield return interval;
            }
        }

        private static IEnumerable<KeyValuePair<DateTime, float>> GenerateTypicalStaffingPercentages(DateTime firstDay, DateTime lastDay)
        {
            firstDay = new DateTime(firstDay.Date.Ticks, DateTimeKind.Local);
            lastDay = new DateTime(lastDay.Date.Ticks, DateTimeKind.Local);
            var current = firstDay;
            current = new DateTime(current.Ticks, DateTimeKind.Local);
            while (current <= lastDay)
            {
                if (current.DayOfWeek.IsWeekday())
                {
                    yield return new KeyValuePair<DateTime, float>(current.AddHours(8).ToUniversalTime(), 0);
                    yield return new KeyValuePair<DateTime, float>(current.AddHours(12).AddHours(5).ToUniversalTime(), 100);
                }
                current = current.AddDays(1);
            }
        }

        private static bool IsWeekend(this DayOfWeek dayOfWeek)
        {
            return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
        }

        private static bool IsWeekday(this DayOfWeek dayOfWeek)
        {
            return !dayOfWeek.IsWeekend();
        }

    }
}
