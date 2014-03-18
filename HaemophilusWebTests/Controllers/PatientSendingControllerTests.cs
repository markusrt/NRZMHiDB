using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingControllerTests
    {
        private readonly SendingController sendingController = SendingControllerTests.CreateMockSendingController();

        [Test]
        public void Create_NewPatientSending_ViewDataContainsSameDataAsIfUsingSendingController()
        {
            var controller = CreateController();

            var expectedResult = sendingController.Create().As<ViewResult>();
            var actualResult = controller.Create().As<ViewResult>();

            actualResult.ViewData.ShouldBeEquivalentTo(expectedResult.ViewData);
        }

        [Test]
        public void Create_InvalidModel_ProducesValidationErrors()
        {
            const string sendingError = "MockError";
            var controller = CreateController();

            controller.Create(new PatientSendingViewModel() {Patient = new Patient(), Sending = new Sending()});

            controller.ModelState.Should().NotBeEmpty();
        }

        private PatientSendingController CreateController()
        {
            return new PatientSendingController(SendingControllerTests.DbMock, new PatientController(), sendingController);
        }
    }
}