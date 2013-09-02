using System;

namespace BetterBurnDown
{
    public interface IPoint
    {
        string Label { get; }
        string Description { get; }
        float Value { get; }
        DateTime Timestamp { get; }
        int? Estimate { get; }
        string Url { get; }
        bool IsProjection { get; }
    }
}