using System.Collections.Generic;

namespace BetterBurnDown
{
    public interface IBurnDownSystem
    {

        IEnumerable<ISprint> GetSprints();

        IGraph GetGraph(string sprintId);

    }
}
