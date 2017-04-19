using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingControllerTests
    {
        private readonly SendingController sendingController = SendingControllerTests.CreateMockSendingController();
        private readonly PatientController patientController = new PatientController(DbMock);

        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        static PatientSendingControllerTests()
        {
            MockData.CreateMockData(DbMock);
        }

        [Test]
        public void Create_NewPatientSending_ViewDataContainsSameDataAsIfUsingSendingController()
        {
            var controller = CreateController();
            var sendingResults = sendingController.Create().As<ViewResult>().ViewData.Keys;
            var patientResults = patientController.Create().As<ViewResult>().ViewData.Keys;
            var expectedResults = sendingResults.Union(patientResults);

            var actualResult = controller.Create().As<ViewResult>().ViewData.Keys;

            actualResult.ShouldBeEquivalentTo(expectedResults);
        }

        [Test]
        public void Create_InvalidModel_ProducesValidationErrors()
        {
            const string sendingError = "MockError";
            var controller = CreateController();

            controller.Create(new PatientSendingViewModel {Patient = new Patient(), Sending = new Sending()});

            controller.ModelState.Should().NotBeEmpty();
        }

        private PatientSendingController CreateController()
        {
            return new PatientSendingController(DbMock, patientController, sendingController);
        }
    }
}