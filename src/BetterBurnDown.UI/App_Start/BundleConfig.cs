using System.Web;
using System.Web.Optimization;

namespace BetterBurnDown.UI
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                "~/Scripts/highcharts/highcharts.js",
                "~/Scripts/highcharts/gray.js"));

            bundles.Add(new ScriptBundle("~/bundles/lib").Include(
                "~/Scripts/lib/json2.js",
                "~/Scripts/lib/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/lib/modernizr-*"));

        }
    }
}