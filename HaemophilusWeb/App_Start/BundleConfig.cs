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
                .Include("~/Scripts/globalize/globalize.js")
                .Include("~/Scripts/globalize/cultures/globalize.culture.de-DE.js"));

            bundles.Add(new ScriptBundle("~/bundles/globalized-validation")
                .Include("~/Scripts/jquery.validate.globalize.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/moment-with-locales.js",
                "~/Scripts/bootstrap-datetimepicker.js",
                "~/Scripts/select2.js",
                "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/site.js"));

            bundles.Add(new ScriptBundle("~/bundles/docxgen")
                .Include("~/Scripts/base64.js")
                .Include("~/Scripts/jszip.js")
                .Include("~/Scripts/FileSaver.js")
                .Include("~/Scripts/docxgen.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-datetimepicker.css",
                "~/Content/select2.css",
                "~/Content/bootstrap-select2.css",
                "~/Content/DataTables/css/dataTables.bootstrap.css",
                "~/Content/site.css"));
        }
    }
}