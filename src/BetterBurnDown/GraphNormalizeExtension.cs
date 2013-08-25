using System;

namespace BetterBurnDown
{
    public static class GraphNormalizeExtension
    {

        public static Graph Normalize(this IGraph graph)
        {
            if (graph == null) throw new ArgumentNullException("graph");
            var normalized = new Graph();
            foreach (var line in graph.Lines)
                Normalize(line, normalized.AddLine(line.Description));
            return normalized;
        }

        private static void Normalize(ILine line, Line normalized)
        {
            var max = line.Max ?? 100.0;
            var stretch = (float)(100.0 / max);
            foreach (var point in line.Points)
                Normalize(point, normalized.AddPoint(point.Value*stretch, point.Timestamp));
        }

        private static void Normalize(IPoint point, Point normalized)
        {
            normalized.Label = point.Label;
            normalized.Description = point.Description;
            normalized.OriginalValue = point.Value;
            normalized.Estimate = point.Estimate;
            normalized.Url = point.Url;
        }

    }
}
