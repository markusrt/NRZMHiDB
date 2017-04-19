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

        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        private static readonly Catalog Catalog = new Catalog();
        private ReportController controller;

        static ReportControllerTests()
        {
            CreateMockData();
        }

        private static void CreateMockData()
        {
            var isolate = Catalog.CreateInstance<Isolate>();
            isolate.ReportDate = DateTime.MinValue;
            isolate.IsolateId = IsolateId;

            DbMock.Isolates.Add(isolate);
        }

        [SetUp]
        public void SetUp()
        {
            controller = new ReportController(DbMock);
        }

        [Test]
        public void ReportGenerated_ValidIsolateId_AssignsReportDate()
        {
            controller.ReportGenerated(IsolateId);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeCloseTo(DateTime.Now);
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