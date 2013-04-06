using System.Collections.Generic;

namespace YouTrackBurnDown.Api
{
    public class SearchResult : ISearchResult
    {
        public List<Issue> Issue { get; set; }
    }
}