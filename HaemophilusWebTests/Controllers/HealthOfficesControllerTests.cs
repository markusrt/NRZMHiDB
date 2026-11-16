using System.Reflection;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Tools;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class HealthOfficesControllerTests
    {
        // specs/016 FR-009, US2.2: opening the edit page from the report page repeats the RKI query; when the website
        // returns no office (or cannot be read), the answer is "not found". The stubbed RKI query keeps the website out
        // of the test, and this branch never touches the database.
        [TestCase("<html><body><div id='result'></div></body></html>")]
        [TestCase("<html><body><div id='result'><select><option>Amt</option></select></div></body></html>")]
        public void Edit_PostalCodeWithoutOfficeOnWebsite_ReturnsNotFound(string rkiPage)
        {
            using (var controller = new HealthOfficesController())
            {
                var queried = new System.Collections.Generic.List<string>();
                typeof(HealthOfficesController)
                    .GetField("rkiTool", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(controller, new RkiTool(postalCode =>
                    {
                        queried.Add(postalCode);
                        return rkiPage;
                    }));

                var result = controller.Edit(null, "91301");

                result.Should().BeOfType<HttpNotFoundResult>();
                queried.Should().Equal("91301");
            }
        }

        // specs/016 US3.3, US3.4 and the edit page: a missing record identifier gives "bad request"
        [Test]
        public void DetailsDeleteAndEdit_WithoutId_ReturnBadRequest()
        {
            using (var controller = new HealthOfficesController())
            {
                ((HttpStatusCodeResult) controller.Details(null)).StatusCode.Should().Be(400);
                ((HttpStatusCodeResult) controller.Delete((int?) null)).StatusCode.Should().Be(400);
                ((HttpStatusCodeResult) controller.Edit(null, null)).StatusCode.Should().Be(400);
            }
        }
    }
}
