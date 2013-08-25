using System.Web.Mvc;
using BetterBurnDown.UI.Models;
using BetterBurnDown.YouTrack;

namespace BetterBurnDown.UI.Controllers
{
    [RequireHttps]
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
            var error = LoginOperation.Login(model.Login, model.Password);
            ModelState.AddModelError(" ", error);
            return RedirectToAction("Index");
        }

    }
}
