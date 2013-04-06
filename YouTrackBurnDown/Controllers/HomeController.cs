using System.Web.Mvc;
using YouTrackBurnDown.Models;

namespace YouTrackBurnDown.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View(BurnDown.Load());
        }

    }
}
