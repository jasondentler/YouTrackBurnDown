using System;
using System.Collections.Generic;

namespace BetterBurnDown
{
    public interface ILine
    {
        string Description { get; }
        float? Max { get; }
        float? Min { get; }
        DateTime? Begin { get; }
        DateTime? End { get; }
        IEnumerable<IPoint> Points { get; }
    }
}