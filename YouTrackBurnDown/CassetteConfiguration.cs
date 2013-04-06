using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace YouTrackBurnDown
{
    /// <summary>
    /// Configures the Cassette asset modules for the web application.
    /// </summary>
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.AddPerSubDirectory<StylesheetBundle>("Content");
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            bundles.AddUrlWithAlias<ScriptBundle>("/signalr/hubs", "~/signalr", bundle => bundle.AddReference("~/Scripts/lib/"));
        }
    }
}