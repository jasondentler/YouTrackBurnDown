using System.Collections.Generic;

namespace YouTrackBurnDown.Api
{
    public class Issue 
    {

        public string Id { get; set; }

        public string JiraId { get; set; }

        public List<Item> Field { get; set; }

    }
}