using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using RestSharp;
using YouTrackBurnDown.Models;

namespace YouTrackBurnDown.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        [HttpGet, ModelStateToTempData]
        public ActionResult Index()
        {
            return View(new AccountCredentials());
        }

        [HttpPost, ModelStateToTempData, ValidateAntiForgeryToken]
        public RedirectToRouteResult Index(AccountCredentials model)
        {
            var request = new RestRequest("rest/user/login", Method.POST);
            request.AddParameter("login", model.Login);
            request.AddParameter("password", model.Password);

            var client = new RestClient();
            client.BaseUrl = GetBaseUrl().ToString();
            
            var response = client.Execute(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    CookieAuthenticator.RedirectFromLogin(model.Login, response.Cookies);
                    return null;
                case HttpStatusCode.Forbidden:
                    dynamic x = JsonConvert.DeserializeObject(response.Content);
                    string error = x.value;
                    ModelState.AddModelError(" ", error);
                    break;
                default:
                    ModelState.AddModelError(" ", string.Format("YouTrack returned {0}: {1}", 
                        (int) response.StatusCode,
                        response.StatusDescription));
                    break;
            }
            return RedirectToAction("Index");
        }


        [NonAction]
        private Uri GetBaseUrl()
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
