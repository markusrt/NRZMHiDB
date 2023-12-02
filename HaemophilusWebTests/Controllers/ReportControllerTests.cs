using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Automapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    public class ReportControllerTests : ITempDirectoryTest
    {
        private const int IsolateId = 10;
        private const string Report1 = "report1.docx";
        private const string Report2 = "report2.docx";

        private static ApplicationDbContextMock DbMock;

        private static readonly Catalog Catalog = new Catalog();

        private ReportController controller;

        public string TemporaryDirectoryToStoreTestData { get; set; }

        private static void CreateMockData()
        {
            var isolate = DbMock.Isolates.SingleOrDefault(i => i.IsolateId == IsolateId)
                ?? Catalog.CreateInstance<Isolate>();
            isolate.ReportDate = null;
            isolate.ReportStatus = ReportStatus.None;
            isolate.IsolateId = IsolateId;

            DbMock.Isolates.Add(isolate);

            var senderId = isolate.Sending.SenderId;
            var sender = DbMock.Senders.SingleOrDefault(s => s.SenderId == senderId)
                ?? Catalog.CreateInstance<Sender>();
            sender.SenderId = senderId;
            DbMock.Senders.Add(sender);
        }

        private void CreateMockReportTemplates()
        {
            CreateMockReportTemplate(Report1);
            CreateMockReportTemplate(Report2);
        }

        private void CreateMockReportTemplate(string name)
        {
            using (var file = File.Create(Path.Combine(TemporaryDirectoryToStoreTestData, name), 1, FileOptions.RandomAccess))
            {
                file.Close();
            }
        }

        [OneTimeSetUp]
        public void InitializeAutomapper()
        {
            MvcApplication.InitializeAutomapper();
        }

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            IsolateViewModelMappingActionBase.DbForTest = DbMock;
            controller = new ReportController(DbMock);
            var context = Substitute.For<HttpContextBase>();
            var server = Substitute.For<HttpServerUtilityBase>();
            server.MapPath(ReportController.ReportTemplatesPath).Returns(TemporaryDirectoryToStoreTestData);
            context.Server.Returns(server);
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            ConfigurationManager.AppSettings["reportSigners"] = "signer1";

            CreateMockReportTemplates();

            CreateMockData();
        }

        [Test]
        public void Isolate_ReportTemplatesAvailable_AddsFileInfosToViewBag()
        {
            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var reportTemplates = viewResult.ViewBag.ReportTemplates as List<FileInfo>;
            reportTemplates.Count.Should().Be(2);
            reportTemplates.Should().Contain(f => f.FullName.EndsWith(Report1));
            reportTemplates.Should().Contain(f => f.FullName.EndsWith(Report2));
        }

        [Test]
        public void Isolate_ValidModel_FillsInterpretationProperties()
        {
            var isolate = DbMock.Isolates.SingleOrDefault(i => i.IsolateId == IsolateId);
            isolate.Agglutination = SerotypeAgg.Negative;
            isolate.BexA = TestResult.NotDetermined;
            isolate.SerotypePcr = SerotypePcr.NotDetermined;
            isolate.Agglutination = SerotypeAgg.Negative;
            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var model = viewResult.Model as IsolateViewModel;
            model.InterpretationPreliminary.Should().Contain("Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).");
            model.InterpretationPreliminary.Should().NotContain("Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
            model.Interpretation.Should().Contain("Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
        }

        [Test]
        public void Isolate_NoReportTemplatesAvailable_AddsFileInfosToViewBag()
        {
            Directory.Delete(TemporaryDirectoryToStoreTestData, true);
            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var reportTemplates = viewResult.ViewBag.ReportTemplates as List<FileInfo>;
            reportTemplates.Count.Should().Be(0);
        }

        [Test]
        public void ReportGenerated_ValidIsolateId_AssignsReportDate()
        {
            controller.ReportGenerated(IsolateId);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
            isolate.ReportStatus.Should().Be(ReportStatus.Final);
        }

        [Test]
        public void ReportGenerated_MultipleReports_UpdatesReportDate()
        {
            controller.ReportGenerated(IsolateId);
            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            var firstReportDate = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
            isolate.ReportDate = firstReportDate;

            controller.ReportGenerated(IsolateId);

            isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeAfter(firstReportDate);
        }

        [Test]
        public void ReportGenerated_ValidIsolateIdAndPreliminaryReport_DoesNotAssignReportDate()
        {
            controller.ReportGenerated(IsolateId, true);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.HasValue.Should().Be(false);
            isolate.ReportStatus.Should().Be(ReportStatus.Preliminary);
        }

        [Test]
        public void ReportGenerated_PreliminaryReportAfterFinal_DoesNotResetStateNorReportDate()
        {
            controller.ReportGenerated(IsolateId);
            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            var originalReportDate = isolate.ReportDate;

            controller.ReportGenerated(IsolateId, true);

            isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().Be(originalReportDate);
            isolate.ReportStatus.Should().Be(ReportStatus.Final);
        }


        [TestCase(1)]
        [TestCase(null)]
        [TestCase(-1)]
        public void ReportGenerated_InvalidIsolateId_DoesNotThrow(int? isolateId)
        {
            controller.Invoking(c => c.ReportGenerated(isolateId)).Should().NotThrow();
        }

    }
}