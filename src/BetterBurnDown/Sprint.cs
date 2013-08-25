using System;

namespace BetterBurnDown
{
    public class Sprint : ISprint
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}