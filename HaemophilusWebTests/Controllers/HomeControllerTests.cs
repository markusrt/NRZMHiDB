using System.Collections.Generic;
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
            var changes = result.Model as IEnumerable<Change>;
            changes.Should().NotBeNull();
            changes.Select(c => c.Date).Should().BeInDescendingOrder();
        }
    }
}