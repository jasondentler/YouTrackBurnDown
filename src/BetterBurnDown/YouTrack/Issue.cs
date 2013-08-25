using System.Collections.Generic;

namespace BetterBurnDown.YouTrack
{
    public class Issue 
    {

        public string Id { get; set; }

        public string JiraId { get; set; }

        public List<Item> Field { get; set; }

    }
}