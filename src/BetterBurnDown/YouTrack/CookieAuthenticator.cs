using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using RestSharp;

namespace BetterBurnDown.YouTrack
{
    public class CookieAuthenticator : IAuthenticator
    {
        private static HttpContextBase Context
        {
            get
            {
                var ctx = HttpContext.Current;
                return new HttpContextWrapper(ctx);
            }
        }

        private static HttpResponseBase Response
        {
            get
            {
                var ctx = Context;
                return ctx == null ? null : ctx.Response;
            }
        }

        private static IPrincipal User
        {
            get
            {
                var ctx = Context;
                return ctx == null ? null : ctx.User;
            }
        }

        public static void RedirectFromLogin(string login, IEnumerable<RestResponseCookie> cookies)
        {
            if (cookies == null)
                throw new ArgumentNullException("cookies");
            
            cookies = cookies.ToArray();

            var data = JsonConvert.SerializeObject(cookies.ToDictionary(c => c.Name, c => c.Value));

            var cookie = FormsAuthentication.GetAuthCookie(login, false);

            var sourceTicket = FormsAuthentication.Decrypt(cookie.Value);

            if (sourceTicket == null)
                throw new ApplicationException("Unable to decrypt authentication");

            var expiration = cookies.Select(c => c.Expires)
                                    .Where(exp => exp > DateTime.Today.AddYears(-1) && exp < DateTime.Today.AddYears(1))
                                    .Concat(new[] {sourceTicket.Expiration})
                                    .Min();

            var ticket = new FormsAuthenticationTicket(
                sourceTicket.Version, sourceTicket.Name, sourceTicket.IssueDate,
                expiration, false, data);

            cookie.Value = FormsAuthentication.Encrypt(ticket);
         
            Response.SetCookie(cookie);
            var redirectUrl = FormsAuthentication.GetRedirectUrl(login, sourceTicket.IsPersistent);
            Response.Redirect(redirectUrl);
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (User == null)
                throw new NotAuthorizedException("No user");

            if (!User.Identity.IsAuthenticated)
                throw new NotAuthorizedException("Not logged in");

            var identity = User.Identity as FormsIdentity;
            if (identity == null)
                throw new NotSupportedException("User.Identity must be a FormsIdentity");

            var ticket = identity.Ticket;
            var cookieData = JsonConvert.DeserializeObject<Dictionary<string, string>>(ticket.UserData);

            foreach (var item in cookieData)
                request.AddCookie(item.Key, item.Value);
        }
    }
}