using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ScreenshotTests
{
    /// <summary>
    /// Assembly-level orchestration for the gold-master screenshot suite. Once per run it:
    /// resets the isolated LocalDB catalog, seeds deterministic data, points the app at that catalog,
    /// starts IIS Express, launches headless Chromium and logs in once (persisting the session as
    /// storage state so every screenshot test reuses it).
    /// </summary>
    [SetUpFixture]
    public class Harness
    {
        private static IPlaywright _playwright;
        private static IBrowser _browser;
        private static IisExpress _iis;
        private static readonly WebConfigConnectionSwap ConfigSwap = new WebConfigConnectionSwap();
        private static string _storageStatePath;

        internal static IBrowser Browser =>
            _browser ?? throw new InvalidOperationException("Playwright browser not initialised.");

        internal static string StorageStatePath => _storageStatePath;

        [OneTimeSetUp]
        public async Task GlobalSetUp()
        {
            LocalDb.DropTestCatalog();
            Seeder.Seed();

            ConfigSwap.Apply();

            _iis = new IisExpress();
            _iis.Start();
            await _iis.WaitUntilReadyAsync(TimeSpan.FromMinutes(3));

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

            _storageStatePath = Path.Combine(Path.GetTempPath(), "haemophilus-screenshot-auth.json");
            await LogInAsync();
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }

            _playwright?.Dispose();
            _iis?.Dispose();
            ConfigSwap.Restore();
        }

        private async Task LogInAsync()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(Paths.BaseUrl + "/Account/Login",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
            await page.FillAsync("#UserName", Seeder.UserName);
            await page.FillAsync("#Password", Seeder.Password);
            await page.ClickAsync("input[type=submit]");

            // The layout renders a #logoutForm only when authenticated. Wait for it to be present in the
            // DOM (Attached, not Visible) so the check is independent of navbar styling — the collapsed
            // navbar hides it mid-migration, which is expected and captured by the screenshots themselves.
            await page.WaitForSelectorAsync("#logoutForm",
                new PageWaitForSelectorOptions { State = WaitForSelectorState.Attached, Timeout = 30000 });

            await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = _storageStatePath });
            await context.CloseAsync();
        }
    }
}
