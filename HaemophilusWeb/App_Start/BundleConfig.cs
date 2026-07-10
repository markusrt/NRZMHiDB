using System.Web.Optimization;

namespace HaemophilusWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquerydatatable").Include(
                "~/Scripts/DataTables/jquery.dataTables.min.js",
                "~/Scripts/DataTables/datetime-moment.js",
                "~/Scripts/DataTables/dataTables.bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/globalize-de-DE")
                .Include("~/Scripts/cldr.js")
                .Include("~/Scripts/cldr/supplemental.js")
                .Include("~/Scripts/cldr/unresolved.js")
                .Include("~/Scripts/cldr/event.js")
                .Include("~/Scripts/globalize.js")
                .Include("~/Scripts/globalize/date.js")
                .Include("~/Scripts/globalize/number.js"));

            bundles.Add(new ScriptBundle("~/bundles/globalized-validation")
                .Include("~/Scripts/jquery.validate.globalize.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            // Bootstrap 5's own JS/CSS are loaded directly in _Layout (pre-minified) because the legacy
            // System.Web.Optimization minifier cannot parse modern (ES6+) syntax. respond.js (IE8 shim)
            // is obsolete under Bootstrap 5 and has been dropped.
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/moment-with-locales.js",
                "~/Scripts/bootstrap-datetimepicker.js",
                "~/Scripts/select2.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/site.js",
                "~/Scripts/imagemodule.js"));

            // bootstrap.min.css is linked directly in _Layout (see note on the bootstrap script bundle).
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap-datetimepicker.css",
                "~/Content/select2.css",
                "~/Content/bootstrap-select2.css",
                "~/Content/DataTables/css/dataTables.bootstrap.css",
                "~/Content/site.css"));
        }
    }
}