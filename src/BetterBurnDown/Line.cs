using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBurnDown
{
    public class Line : ILine
    {

        private readonly List<Point> _points = new List<Point>();

        public Line(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
        public float? Min { get { return _points.Any(p => !float.IsNaN(p.Value)) ? _points.Where(p => !float.IsNaN(p.Value)).Min(p => p.Value) : (float?)null; } }
        public float? Max { get { return _points.Any(p => !float.IsNaN(p.Value)) ? _points.Where(p => !float.IsNaN(p.Value)).Max(p => p.Value) : (float?)null; } }
        public DateTime? Begin { get { return _points.Any() ? _points.Min(p => p.Timestamp) : (DateTime?)null; } }
        public DateTime? End { get { return _points.Any() ? _points.Max(p => p.Timestamp) : (DateTime?)null; } }
        public IEnumerable<IPoint> Points { get { return _points.AsEnumerable(); } }

        private Point _lastPoint;

        public Point AddPoint(float value, DateTime timestamp)
        {
            var p = new Point(value, timestamp);
            _points.Add(p);
            _lastPoint = p;
            return p;
        }

        public Point AddPointByChange(float valueChange, DateTime timestamp)
        {
            var last = _lastPoint; //_points.FindLast(p => p.Timestamp <= timestamp);
            return AddPoint(last.Value + valueChange, timestamp);
        }

    }
}