using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataTables.Mvc;
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
        private SendingController sendingController;
        private PatientController patientController;
        private ApplicationDbContextMock DbMock;
        private NameValueCollection requestForm = new NameValueCollection();

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            MockData.CreateMockData(DbMock);
            patientController = new PatientController(DbMock);
            sendingController = new SendingController(DbMock);
            foreach (var sending in DbMock.Sendings)
            {
                sending.Isolate = MockData.CreateInstance<Isolate>();
            }
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

            controller.Create(new PatientSendingViewModel {Patient = new Patient(), Sending = new Sending()});

            controller.ModelState.Should().NotBeEmpty();
        }

        [Test]
        public void Create_AssignsClinicalInformation()
        {
            var controller = CreateController();
            requestForm.Set("ClinicalInformation", "Pneumonia,Sepsis");

            var patient = MockData.CreateInstance<Patient>();
            controller.Create(new PatientSendingViewModel {Patient = patient, Sending = MockData.CreateInstance<Sending>()});

            var patientInDatabase = DbMock.Patients.Last();
            patientInDatabase.Initials.Should().Be(patient.Initials);
            patientInDatabase.ClinicalInformation.Should().Be(ClinicalInformation.Sepsis|ClinicalInformation.Pneumonia);
        }


        [TestCase(ReportStatus.None, "glyphicon-remove-sign")]
        [TestCase(ReportStatus.Preliminary, "glyphicon-time")]
        [TestCase(ReportStatus.Final, "glyphicon-ok-sign")]
        public void DataTableAjax_ReportGenerated_IsSetAccordingToReportState(ReportStatus reportStatus, string expectedIcon)
        {
            var controller = CreateController();
            var firstIsolate = DbMock.Sendings.First(s => !s.Deleted).Isolate;
            firstIsolate.ReportStatus = reportStatus;
            var request = new DefaultDataTablesRequest {Search = new Search("", false), Columns = new ColumnCollection(new List<Column>()), Length = 10};

            var result = controller.DataTableAjax(request);

            var firstRow = ((DataTablesResponse)result.Data).data.Cast<dynamic>().First();
            ((string)firstRow.ReportGenerated).Should().Contain(expectedIcon);
        }

        private PatientSendingController CreateController()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(r => r.Form).Returns(requestForm);
            var context = new Mock<HttpContextBase>();
            var server = new Mock<HttpServerUtilityBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Server).Returns(server.Object);
            var controller = new PatientSendingController(DbMock, patientController, sendingController);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            var urlHelper = new Mock<UrlHelper>();
            urlHelper.Setup(u => u.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(string.Empty);
            urlHelper.Setup(u => u.Action(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(string.Empty);

            controller.Url = urlHelper.Object;
            return controller;
        }
    }
}