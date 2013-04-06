using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using RestSharp;

namespace YouTrackBurnDown.Api
{
    public class Client
    {
        private readonly Uri _baseUrl;

        public Client()
            : this(ConfigurationManager.AppSettings["server"])
        {
        }

        public Client(string server)
            : this(new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps,
                    Host = server,
                    Path = "/youtrack"
                }.Uri)
        {
        }

        public Client(Uri baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = _baseUrl.ToString();
            client.Authenticator = new CookieAuthenticator();
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
                throw response.ErrorException;
            return response.Data;
        }

        public string Execute(RestRequest request)
        {
            var client = new RestClient();
            client.BaseUrl = _baseUrl.ToString();
            client.Authenticator = new CookieAuthenticator();
            var response = client.Execute(request);
            if (response.ErrorException != null)
                throw response.ErrorException;
            return response.Content;
        }

        public IEnumerable<Project> GetProjects()
        {
            return Execute<List<Project>>(new RestRequest("/rest/project/all", Method.GET));
        }

        public ISearchResult QueryIssues(string[] filters, string[] fields, int pageSize, int pageNumber)
        {
            var request = new RestRequest("/rest/issue");
            foreach (var filter in filters)
                request.AddParameter("filter", filter, ParameterType.GetOrPost);
            foreach (var field in fields)
                request.AddParameter("with", field, ParameterType.GetOrPost);
            request.AddParameter("max", pageSize);
            request.AddParameter("after", (pageNumber - 1)*pageSize);

            switch (filters.Length)
            {
                case 0:
                    return null;
                case 1:
                    return Execute<SearchResult>(request);
                default:
                    return Execute<MultiSearchResult>(request);
            }
        }

        public Tuple<string, string, DateTime, DateTime> GetSprintDates(string projectShortName, string sprint)
        {
            var request = new RestRequest("/rest/agile/{project}/sprint/{sprint}/settings");
            request.AddUrlSegment("project", projectShortName);
            request.AddUrlSegment("sprint", sprint);
            var result = Execute<SprintDates>(request);
            return Tuple.Create(projectShortName, sprint, Convert(result.Start), Convert(result.Finish));
        }

        private static DateTime Convert(long milliseconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(milliseconds);
        }


    }
}