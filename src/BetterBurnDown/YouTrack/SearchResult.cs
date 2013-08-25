using System.Collections.Generic;

namespace BetterBurnDown.YouTrack
{
    public class SearchResult : ISearchResult
    {
        public List<Issue> Issue { get; set; }
    }
}