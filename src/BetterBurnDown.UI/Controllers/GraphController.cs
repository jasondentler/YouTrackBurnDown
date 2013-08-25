using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using BetterBurnDown.YouTrack;

namespace BetterBurnDown.UI.Controllers
{
    public class GraphController : ApiController
    {

        public IEnumerable<ISprint> Get()
        {
            var sprints =  new DavidWeekleyConventions().GetSprints();
            return sprints.Cast<ISprint>();
        }

        public IGraph Get(string id)
        {
            var result = new DavidWeekleyConventions().GetGraph(id);
            return result;
        }

    }
}