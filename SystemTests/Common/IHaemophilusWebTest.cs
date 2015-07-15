using System;
using OpenQA.Selenium.Remote;

namespace SystemTests.Common
{
    /// <summary>
    ///     Attaching this interface to a NUnit test class provides access
    ///     to a web driver which is logged in to the Haemophilus Web site.
    /// </summary>
    [HaemophilusWebTest]
    public interface IHaemophilusWebTest
    {
        RemoteWebDriver Driver { get; set; }

        Action<string> GoToUrl { get; set; }
    }
}