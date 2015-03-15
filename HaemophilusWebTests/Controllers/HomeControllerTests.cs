using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public void Index()
        {
            var controller = new HomeController();

            var result = controller.Index() as ViewResult;

            result.Should().NotBeNull();
        }

        [Test]
        public void About()
        {
            var controller = new HomeController();

            var result = controller.About() as ViewResult;

            result.Should().NotBeNull();
        }

        [Test]
        public void Changes_AreOrderedDescending()
        {
            var controller = new HomeController();

            var result = controller.About() as ViewResult;

            result.Should().NotBeNull();
            var changeLog = result.Model as ChangeLog;
            var previousChanges = changeLog.PreviousChanges;
            previousChanges.Should().NotBeNull();
            previousChanges.Select(c => c.Date).Should().BeInDescendingOrder();
        }
    }
}