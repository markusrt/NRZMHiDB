using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ScreenshotTests
{
    /// <summary>
    /// Captures a full-page screenshot of every route in <see cref="Routes"/> into
    /// screenshots/&lt;mode&gt;/. Run with SCREENSHOT_MODE=baseline to record the gold master and with
    /// SCREENSHOT_MODE=current (default) to record the set under test; <see cref="GalleryReportTest"/>
    /// then diffs the two.
    /// </summary>
    [TestFixture]
    public class GoldMasterTests
    {
        private static readonly ViewportSize Desktop = new ViewportSize { Width = 1366, Height = 900 };
        private static readonly ViewportSize Mobile = new ViewportSize { Width = 390, Height = 844 };

        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (var route in Routes.All)
            {
                yield return new TestCaseData(route, "desktop").SetName($"{route.Name}__desktop");
                if (route.Mobile)
                {
                    yield return new TestCaseData(route, "mobile").SetName($"{route.Name}__mobile");
                }
            }
        }

        [TestCaseSource(nameof(Cases))]
        public async Task Capture(Route route, string viewport)
        {
            var outputDir = Paths.ScreenshotDir(Paths.Mode);
            Directory.CreateDirectory(outputDir);

            var contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = viewport == "mobile" ? Mobile : Desktop,
                DeviceScaleFactor = 1,
                Locale = "de-DE"
            };
            if (!route.Anonymous)
            {
                contextOptions.StorageStatePath = Harness.StorageStatePath;
            }

            await using var context = await Harness.Browser.NewContextAsync(contextOptions);
            var page = await context.NewPageAsync();

            await page.GotoAsync(Paths.BaseUrl + route.Url,
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

            await StabilizeAsync(page);

            var path = Path.Combine(outputDir, $"{route.Name}__{viewport}.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true, Animations = ScreenshotAnimations.Disabled });

            Assert.That(File.Exists(path), Is.True, $"Screenshot was not written for {route.Name} ({viewport}).");
        }

        /// <summary>Removes remaining sources of run-to-run noise (dynamic dates, caret blinks, animations).</summary>
        private static async Task StabilizeAsync(IPage page)
        {
            await page.EvaluateAsync(@"() => {
                // Freeze any date/time inputs to a constant so 'today' defaults don't create false diffs.
                document.querySelectorAll('input.datepicker, input.datetimepicker, input[type=datetime], input[type=date]')
                    .forEach(el => { el.value = '01.01.2020'; });
                // Disable CSS transitions/animations and caret blink.
                const style = document.createElement('style');
                style.textContent = '*{ transition:none !important; animation:none !important; caret-color:transparent !important; }';
                document.head.appendChild(style);
            }");
            // Let layout settle after mutations.
            await page.WaitForTimeoutAsync(200);
        }
    }
}
