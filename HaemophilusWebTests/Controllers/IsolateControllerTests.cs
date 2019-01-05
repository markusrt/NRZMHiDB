using System;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Automapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    public class IsolateControllerTests
    {
        private const int IsolateId = 10;

        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        private static readonly Catalog Catalog = new Catalog();
        private IsolateController controller;

        static IsolateControllerTests()
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
            controller = new IsolateController(DbMock);
        }

        [Test]
        public void ParseAndMapLaboratoryNumber_ValidLaboratoryNumber_AssignsYearAndSequentialNumberCorrectly()
        {
            var isolateViewModel = new IsolateViewModel {LaboratoryNumber = "0000123/15"};
            var isolate = new Isolate();

            IsolateViewModelMappingActionBase.ParseAndMapLaboratoryNumber(isolateViewModel, isolate);

            isolate.Year.Should().Be(2015);
            isolate.YearlySequentialIsolateNumber.Should().Be(123);
        }
    }
}