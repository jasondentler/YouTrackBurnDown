using System.Collections.Generic;

namespace BetterBurnDown.YouTrack
{
    public class MultiSearchResult : ISearchResult
    {

        public class SearchResultItem
        {
            public string Search { get; set; }
            public List<Issue> Issues { get; set; }
        }

        public List<SearchResultItem> SearchResult { get; set; }

    }
}