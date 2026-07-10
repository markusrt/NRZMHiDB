using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ScreenshotTests
{
    /// <summary>
    /// Functional check for the Tempus Dominus 6 date picker: the gold-master captures only the closed
    /// field, so this test opens the picker and asserts the calendar widget actually appears (and saves a
    /// screenshot of it for manual review under screenshots/&lt;mode&gt;/).
    /// </summary>
    [TestFixture]
    public class DatePickerTest
    {
        [Test]
        public async Task Picker_Opens_OnToggleClick()
        {
            await using var context = await Harness.Browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1366, Height = 900 },
                StorageStatePath = Harness.StorageStatePath,
                Locale = "de-DE"
            });
            var page = await context.NewPageAsync();

            // Patient/Create renders a BirthDate field via DateEditorFor (Tempus Dominus).
            await page.GotoAsync(Paths.BaseUrl + "/Patient/Create",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

            var toggle = page.Locator("[data-td-toggle='datetimepicker']").First;
            await toggle.ClickAsync();

            var widget = page.Locator(".tempus-dominus-widget").First;
            await widget.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

            var dir = Paths.ScreenshotDir(Paths.Mode);
            Directory.CreateDirectory(dir);
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(dir, "_DatePicker_open.png"),
                FullPage = true
            });

            Assert.That(await widget.IsVisibleAsync(), Is.True, "Tempus Dominus calendar widget did not open.");
        }
    }
}
