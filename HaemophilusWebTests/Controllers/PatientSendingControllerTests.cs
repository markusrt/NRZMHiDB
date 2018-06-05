using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using Moq;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingControllerTests
    {
        private readonly SendingController sendingController = SendingControllerTests.CreateMockSendingController();
        private readonly PatientController patientController = new PatientController(DbMock);

        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();
        private NameValueCollection requestForm = new NameValueCollection();

        static PatientSendingControllerTests()
        {
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
            var controller = CreateController();

            controller.Create(new PatientSendingViewModel<Patient,Sending> {Patient = new Patient(), Sending = new Sending()});

            controller.ModelState.Should().NotBeEmpty();
        }

        [Test]
        public void Create_AssignsClinicalInformation()
        {
            var controller = CreateController();
            requestForm.Set("ClinicalInformation", "Pneumonia,Sepsis");

            var patient = MockData.CreateInstance<Patient>();
            controller.Create(new PatientSendingViewModel<Patient, Sending> { Patient = patient, Sending = MockData.CreateInstance<Sending>()});

            var patientInDatabase = DbMock.Patients.Last();
            patientInDatabase.Initials.Should().Be(patient.Initials);
            patientInDatabase.ClinicalInformation.Should().Be(ClinicalInformation.Sepsis|ClinicalInformation.Pneumonia);
        }

        private PatientSendingController CreateController()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(r => r.Form).Returns(requestForm);
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            var controller = new PatientSendingController(DbMock, patientController, sendingController);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            return controller;
        }
    }
}