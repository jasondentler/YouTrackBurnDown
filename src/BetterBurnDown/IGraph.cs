using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BetterBurnDown
{
    public interface IGraph
    {
        IEnumerable<ILine> Lines { get; }
        float? Max { get; }
        float? Min { get; }
        DateTime? Begin { get; }
        DateTime? End { get; }
    }
}
