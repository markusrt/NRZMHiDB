using System;
using System.IO;

namespace ScreenshotTests
{
    /// <summary>
    /// Central location for all paths, the app URL and the isolated test database used by the
    /// gold-master screenshot harness. Everything is derived from the repository root so the
    /// harness works from any checkout and on CI.
    /// </summary>
    internal static class Paths
    {
        // Dedicated, disposable LocalDB catalog (kept in sync with app.config / the Web.config swap).
        public const string TestCatalog = "HaemophilusWeb_Screenshots";

        public const string LocalDbInstance = @"(LocalDB)\MSSQLLocalDB";

        public static string ConnectionString =>
            $@"Data Source={LocalDbInstance};Initial Catalog={TestCatalog};Integrated Security=True;MultipleActiveResultSets=True";

        // http on localhost avoids the (localhost-exempt) HTTPS redirect rule and the cert dance.
        public const int Port = 9899;

        public static string BaseUrl => $"http://localhost:{Port}";

        private static readonly Lazy<string> RepoRootLazy = new Lazy<string>(FindRepoRoot);

        public static string RepoRoot => RepoRootLazy.Value;

        public static string WebAppDir => Path.Combine(RepoRoot, "HaemophilusWeb");

        public static string WebConfig => Path.Combine(WebAppDir, "Web.config");

        public static string IisExpressExe =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express", "iisexpress.exe");

        public static string ScreenshotsRoot => Path.Combine(RepoRoot, "ScreenshotTests", "screenshots");

        public static string ReportRoot => Path.Combine(RepoRoot, "ScreenshotTests", "report");

        // "baseline" or "current" – selects which screenshot set is written. Defaults to current.
        public static string Mode =>
            (Environment.GetEnvironmentVariable("SCREENSHOT_MODE") ?? "current").Trim().ToLowerInvariant();

        public static string ScreenshotDir(string mode) => Path.Combine(ScreenshotsRoot, mode);

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "HaemophilusWeb.sln")))
            {
                dir = dir.Parent;
            }

            if (dir == null)
            {
                throw new InvalidOperationException(
                    "Could not locate repository root (HaemophilusWeb.sln) above " + AppContext.BaseDirectory);
            }

            return dir.FullName;
        }
    }
}
