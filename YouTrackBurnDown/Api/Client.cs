using System;
using System.Collections.Generic;
using System.Configuration;
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


    }
}