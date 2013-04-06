using System.Collections.Generic;

namespace YouTrackBurnDown.Api
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