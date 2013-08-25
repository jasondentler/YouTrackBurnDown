using System;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace BetterBurnDown.YouTrack
{
    public class LoginOperation
    {

        public static string Login(string login, string password)
        {
            var request = new RestRequest("rest/user/login", Method.POST);
            request.AddParameter("login", login);
            request.AddParameter("password", password);

            var client = new RestClient();
            client.BaseUrl = GetBaseUrl().ToString();

            var response = client.Execute(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    CookieAuthenticator.RedirectFromLogin(login, response.Cookies);
                    return null;
                case HttpStatusCode.Forbidden:
                    dynamic x = JsonConvert.DeserializeObject(response.Content);
                    string error = x.value;
                    return error;
                default:
                    return string.Format("YouTrack returned {0}: {1}",
                                         (int) response.StatusCode,
                                         response.StatusDescription);
            }
        }

        private static Uri GetBaseUrl()
        {
            var server = ConfigurationManager.AppSettings["server"];
            var bldr = new UriBuilder(Uri.UriSchemeHttps, server)
            {
                Path = "/youtrack"
            };

            return bldr.Uri;
        }

    }
}
