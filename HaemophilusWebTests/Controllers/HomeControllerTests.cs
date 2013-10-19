using System.Web.Mvc;
using FluentAssertions;
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
        public void Contact()
        {
            var controller = new HomeController();

            var result = controller.Contact() as ViewResult;

            result.Should().NotBeNull();
        }
    }
}