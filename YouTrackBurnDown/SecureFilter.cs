using System;
using System.Web.Mvc;

namespace YouTrackBurnDown
{

    public class SecureFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if !DEBUG
            var request = filterContext.RequestContext.HttpContext.Request;
            var isSecure = request.IsSecureConnection;
            if (!isSecure)
            {
                var builder = new UriBuilder(request.Url)
                    {
                        Scheme = Uri.UriSchemeHttps,
                        Port = 443
                    };
                var secureUrl = builder.Uri.ToString();
                filterContext.Result = new RedirectResult(secureUrl);
                return;
            }
#endif
            base.OnActionExecuting(filterContext);
        }

    }

}