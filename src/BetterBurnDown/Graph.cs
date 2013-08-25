using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBurnDown
{
    public class Graph : IGraph
    {

        private readonly List<Line> _lines = new List<Line>();

        public IEnumerable<ILine> Lines { get { return _lines.AsEnumerable(); } }

        public float? Min
        {
            get
            {
                var values = _lines.Where(l => l.Min != null && !float.IsNaN((float) l.Min)).ToArray();
                return values.Any()
                           ? values.Min(l => l.Min)
                           : null;
            }
        }

        public float? Max
        {
            get
            {
                var values = _lines.Where(l => l.Max != null && !float.IsNaN((float)l.Max)).ToArray();
                return values.Any()
                           ? values.Max(l => l.Max)
                           : null;
            }
        }

        public DateTime? Begin
        {
            get
            {
                return _lines.Any(l => l.Begin != null) ? _lines.Where(l => l.Begin != null).Min(l => l.Begin) : null;
            }
        }

        public DateTime? End
        {
            get
            {
                return _lines.Any(l => l.End != null) ? _lines.Where(l => l.End != null).Max(l => l.End) : null;
            }
        }

        public Line AddLine(string description)
        {
            var line = new Line(description);
            _lines.Add(line);
            return line;
        }

    }
}
