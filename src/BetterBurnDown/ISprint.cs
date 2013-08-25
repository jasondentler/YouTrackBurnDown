using System;

namespace BetterBurnDown
{
    public interface ISprint
    {

        string Id { get; }
        DateTime Start { get; }
        DateTime End { get; }

    }
}
