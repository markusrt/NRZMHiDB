using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataTables.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingControllerTests : ITempDirectoryTest
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
                sending.Isolate.EpsilometerTests.Clear();
                sending.Isolate.EpsilometerTests.Add(MockData.CreateInstance<EpsilometerTest>());
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

            actualResult.Should().BeEquivalentTo(expectedResults);
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

        [Test]
        public void LaboratoryExport_EmptyQuery_ForwardToViewWithQueryForLastYear()
        {
            var controller = CreateController();

            var result = controller.LaboratoryExport(new FromToQuery());

            var viewResult= result.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            var query = viewResult.Model.Should().BeOfType<FromToQuery>().And.Subject.As<FromToQuery>();
            query.From.Month.Should().Be(1);
            query.From.Year.Should().Be(DateTime.Now.Year - 1);
            query.To.Month.Should().Be(12);
            query.To.Year.Should().Be(DateTime.Now.Year - 1);
        }

        [Test]
        public void LaboratoryExport_ValidQuery_CreatesExcel()
        {
            var tempExcel = new FileInfo(Path.Combine(TemporaryDirectoryToStoreTestData, "LaboratoryExport.xlsx"));
            var controller = CreateController();

            var result = controller.LaboratoryExport(new FromToQuery {From = DateTime.Now.AddYears(-100), To = DateTime.Now});

            var fileResult = result.Should().BeOfType<FileContentResult>().And.Subject.As<FileContentResult>();
            fileResult.FileContents.Length.Should().BeGreaterThanOrEqualTo(10);
            File.WriteAllBytes(tempExcel.FullName, fileResult.FileContents);

            using (var fastExcel = new FastExcel.FastExcel(tempExcel))
            {
                fastExcel.Worksheets.Length.Should().Be(1);
            }
        }

        [Test]
        public void PubMlstExport_EmptyQuery_ForwardToViewWithQueryForLastYear()
        {
            var controller = CreateController();

            var result = controller.LaboratoryExport(new FromToQuery());

            var viewResult= result.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            var query = viewResult.Model.Should().BeOfType<FromToQuery>().And.Subject.As<FromToQuery>();
            query.From.Month.Should().Be(1);
            query.From.Year.Should().Be(DateTime.Now.Year - 1);
            query.To.Month.Should().Be(12);
            query.To.Year.Should().Be(DateTime.Now.Year - 1);
        }

        [Test]
        public void PubMlstExport_ValidQuery_CreatesExcel()
        {
            var tempExcel = new FileInfo(Path.Combine(TemporaryDirectoryToStoreTestData, "PubMLSTExport.xlsx"));
            var controller = CreateController();

            var result = controller.PubMlstExport(new FromToQueryWithAdjustment {From = DateTime.Now.AddYears(-100), To = DateTime.Now});

            var fileResult = result.Should().BeOfType<FileContentResult>().And.Subject.As<FileContentResult>();
            fileResult.FileContents.Length.Should().BeGreaterThanOrEqualTo(10);
            File.WriteAllBytes(tempExcel.FullName, fileResult.FileContents);

            using (var fastExcel = new FastExcel.FastExcel(tempExcel))
            {
                fastExcel.Worksheets.Length.Should().Be(1);
            }
        }

        [Test]
        public void RkiExport_EmptyQuery_ForwardToViewWithQueryForLastYear()
        {
            var controller = CreateController();

            var result = controller.RkiExport(new FromToQueryWithAdjustment());

            var viewResult = result.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            var query = viewResult.Model.Should().BeOfType<FromToQueryWithAdjustment>().And.Subject.As<FromToQuery>();
            query.From.Month.Should().Be(1);
            query.From.Year.Should().Be(DateTime.Now.Year - 1);
            query.To.Month.Should().Be(12);
            query.To.Year.Should().Be(DateTime.Now.Year - 1);
        }

        [Test]
        public void RkiExport_ValidQuery_CreatesExcel()
        {
            var tempExcel = new FileInfo(Path.Combine(TemporaryDirectoryToStoreTestData, "LaboratoryExport.xlsx"));
            var controller = CreateController();

            var result = controller.RkiExport(new FromToQueryWithAdjustment { From = DateTime.Now.AddYears(-100), To = DateTime.Now });

            var fileResult = result.Should().BeOfType<FileContentResult>().And.Subject.As<FileContentResult>();
            fileResult.FileContents.Length.Should().BeGreaterThanOrEqualTo(10);
            File.WriteAllBytes(tempExcel.FullName, fileResult.FileContents);

            using (var fastExcel = new FastExcel.FastExcel(tempExcel))
            {
                fastExcel.Worksheets.Length.Should().Be(1);
            }
        }

        private PatientSendingController CreateController()
        {
            var request = Substitute.For<HttpRequestBase>();
            request.Form.Returns(requestForm);
            var context = Substitute.For<HttpContextBase>();
            var server = Substitute.For<HttpServerUtilityBase>();
            context.Request.Returns(request);
            context.Server.Returns(server);
            var controller = new PatientSendingController(DbMock, patientController, sendingController);
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            var urlHelper = Substitute.For<UrlHelper>();
            urlHelper.Action(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);
            urlHelper.Action(Arg.Any<string>(), Arg.Any<object>()).Returns(string.Empty);

            controller.Url = urlHelper;
            return controller;
        }

        public string TemporaryDirectoryToStoreTestData { get; set; }
    }
}