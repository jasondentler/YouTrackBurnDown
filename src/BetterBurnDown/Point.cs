using System;

namespace BetterBurnDown
{
    public class Point : IPoint
    {

        public Point(float value, DateTime timestamp)
        {
            if (float.IsNaN(value))
                value = 0;
            Value = value;
            Timestamp = timestamp;
        }

        public string Label { get; set; }
        public string Description { get; set; }
        public float Value { get; private set; }
        public DateTime Timestamp { get; private set; }
        public int? Estimate { get; set; }
        public string Url { get; set; }
        public float OriginalValue { get; set; }
    }
}