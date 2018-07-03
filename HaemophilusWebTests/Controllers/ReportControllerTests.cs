using System;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    public class ReportControllerTests
    {
        private const int IsolateId = 10;

        private static ApplicationDbContextMock DbMock;

        private static readonly Catalog Catalog = new Catalog();
        private ReportController controller;

        private static void CreateMockData()
        {
            var isolate = DbMock.Isolates.SingleOrDefault(i => i.IsolateId == IsolateId)
                ?? Catalog.CreateInstance<Isolate>();
            isolate.ReportDate = null;
            isolate.ReportStatus = ReportStatus.None;
            isolate.IsolateId = IsolateId;

            DbMock.Isolates.Add(isolate);
        }

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            controller = new ReportController(DbMock);
            CreateMockData();
        }

        [Test]
        public void ReportGenerated_ValidIsolateId_AssignsReportDate()
        {
            controller.ReportGenerated(IsolateId);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeCloseTo(DateTime.Now);
            isolate.ReportStatus.Should().Be(ReportStatus.Final);
        }

        [Test]
        public void ReportGenerated_MultipleReports_UpdatesReportDate()
        {
            controller.ReportGenerated(IsolateId);
            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            var firstReportDate = isolate.ReportDate;

            controller.ReportGenerated(IsolateId);

            isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeAfter(firstReportDate.Value);
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
            controller.Invoking(c => c.ReportGenerated(isolateId)).ShouldNotThrow();
        }
    }
}