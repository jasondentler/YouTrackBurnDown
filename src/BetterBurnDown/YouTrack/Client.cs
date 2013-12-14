using System;
using System.Collections.Generic;
using System.Configuration;
using RestSharp;

namespace BetterBurnDown.YouTrack
{
    public class Client
    {

        static Client()
        {
            Authenticator = new CookieAuthenticator();
        }

        public static IAuthenticator Authenticator { get; set; }

        private readonly IAuthenticator _authenticator;
        private readonly Uri _baseUrl;

        public Client() : this(Authenticator)
        {
        }

        public Client(IAuthenticator authenticator)
            : this(ConfigurationManager.AppSettings["server"], authenticator)
        {
        }

        public Client(string server, IAuthenticator authenticator)
            : this(new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps,
                    Host = server,
                    Path = "/youtrack"
                }.Uri, authenticator)
        {
        }

        public Client(Uri baseUrl, IAuthenticator authenticator)
        {
            _baseUrl = baseUrl;
            _authenticator = authenticator;
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = _baseUrl.ToString();
            client.Authenticator = _authenticator;

            request.RequestFormat = DataFormat.Json;

            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
                throw response.ErrorException;
            return response.Data;
        }

        public string Execute(RestRequest request)
        {
            var client = new RestClient();
            client.BaseUrl = _baseUrl.ToString();
            client.Authenticator = _authenticator;
            var response = client.Execute(request);
            if (response.ErrorException != null)
                throw response.ErrorException;
            return response.Content;
        }

        public IEnumerable<Project> GetProjects()
        {
            return Execute<List<Project>>(new RestRequest("/rest/project/all", Method.GET));
        }

        public VersionBundle GetVersionBundle(string bundleName)
        {
            var url = "/rest/admin/customfield/versionBundle/" + Uri.EscapeUriString(bundleName);
            var bundle = Execute<VersionBundle>(new RestRequest(url , Method.GET));
            return bundle;
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

        public Sprint GetSprintDates(string projectName, string sprint)
        {
            var request = new RestRequest("/rest/agile/{project}/sprint/{sprint}/settings");
            request.AddUrlSegment("project", projectName);
            request.AddUrlSegment("sprint", sprint);
            var result = Execute<SprintSettings>(request);
            return new Sprint()
                {
                    Id = result.Name,
                    Start = Convert(result.Start),
                    End = Convert(result.Finish)
                };
        }

        public static DateTime Convert(long milliseconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(milliseconds);
        }


    }
}