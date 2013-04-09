using System;
using System.Globalization;
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

            var selectedSprint = "";

            if (string.IsNullOrWhiteSpace(Request.QueryString["SelectedSprint"]))
            {

                var ci = System.Threading.Thread.CurrentThread.CurrentCulture;

                var weekNumber = ci.Calendar.GetWeekOfYear(DateTime.Today,
                                                           CalendarWeekRule.FirstFullWeek,
                                                           DayOfWeek.Tuesday) + 1;

                var selectedSprintYear = DateTime.Today.Year%100;
                var selectedSprintNumber = Convert.ToInt32(Math.Floor(weekNumber*0.5));

                selectedSprint = string.Format("{0,2:D2}{1,2:D2}", selectedSprintYear, selectedSprintNumber);
            }
            else
            {
                selectedSprint = Request.QueryString["SelectedSprint"];
            }

            var burndown = BurnDown.Load();

            burndown.SelectedSprint = selectedSprint;

            return View(burndown);
        }

    }
}
