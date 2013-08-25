using System.Web.Mvc;

namespace BetterBurnDown.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
