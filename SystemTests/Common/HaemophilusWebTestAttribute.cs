using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium.Firefox;

namespace SystemTests.Common
{
    public class HaemophilusWebTestAttribute : TestActionAttribute
    {
        private const string BaseUrl = "https://haemophilus.azurewebsites.net";

        private readonly FirefoxDriver driver =
            new FirefoxDriver(new FirefoxBinary(@"C:\PortableApps\FirefoxPortable\App\Firefox\firefox.exe"),
                new FirefoxProfile());

        public override ActionTargets Targets
        {
            get { return ActionTargets.Suite; }
        }

        public override void BeforeTest(ITest test)
        {
            var haemophilusWebTest = test.Fixture as IHaemophilusWebTest;
            if (haemophilusWebTest == null)
            {
                return;
            }

            haemophilusWebTest.Driver = driver;
            haemophilusWebTest.GoToUrl = GoToUrl;

            Login_CorrectUserAndPassword_Success();
        }

        public override void AfterTest(ITest test)
        {
            driver.Quit();
        }

        private void Login_CorrectUserAndPassword_Success()
        {
            GoToUrl("/Account/Login");

            driver.FindElementById("UserName").SendKeys("Labor1");
            driver.FindElementById("Password").SendKeys("123456");
            driver.FindElementById("Password").Submit();

            driver.FindElementByClassName("navbar-right").Text.Should().Contain("Abmelden");
        }

        private void GoToUrl(string path)
        {
            driver.Navigate().GoToUrl(BaseUrl + path);
        }
    }
}