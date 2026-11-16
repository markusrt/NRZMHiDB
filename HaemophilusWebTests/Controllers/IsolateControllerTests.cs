using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
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

        // specs/004 FR-017: the edit form without an id is a bad request
        [Test]
        public void Edit_WithoutId_ReturnsBadRequest()
        {
            var result = controller.Edit((int?) null);

            ((HttpStatusCodeResult) result).StatusCode.Should().Be((int) HttpStatusCode.BadRequest);
        }

        // specs/004 FR-017: an unknown isolate is not found
        [Test]
        public void Edit_UnknownId_ReturnsNotFound()
        {
            var result = controller.Edit(int.MaxValue);

            result.Should().BeOfType<HttpNotFoundResult>();
        }

        // specs/004 FR-016: every action requires the standard laboratory-user role
        [Test]
        public void Controller_RequiresStandardRole()
        {
            typeof(IsolateController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single().Roles.Should().Be(DefaultRoles.User);
        }

        // specs/004 FR-018: the save requires the anti-forgery token
        [Test]
        public void EditPost_ValidatesAntiForgeryToken()
        {
            typeof(IsolateController).GetMethods()
                .Single(m => m.Name == "Edit" && m.IsDefined(typeof(HttpPostAttribute), true))
                .IsDefined(typeof(ValidateAntiForgeryTokenAttribute), true).Should().BeTrue();
        }

        // specs/004 FR-012: the unique indexes report the conflict against the matching field
        [TestCase("IX_StemNumber", "StemNumber", "Diese Stammnummer ist bereits vergeben")]
        [TestCase("IX_LaboratoryNumber", "LaboratoryNumber", "Diese Labornummer ist bereits vergeben")]
        public void HandleDbUpdateException_DuplicateIndex_AddsErrorToField(string index, string field, string message)
        {
            var sut = new DuplicateHandlingController();

            sut.Handle(new DbUpdateException("update failed", new Exception($"duplicate key in {index}")));

            sut.ModelState[field].Errors.Single().ErrorMessage.Should().Be(message);
        }

        // specs/004 FR-012: other update failures are not reported as duplicates
        [Test]
        public void HandleDbUpdateException_OtherFailure_Rethrows()
        {
            var sut = new DuplicateHandlingController();
            var exception = new DbUpdateException("update failed", new Exception("other"));

            Action handle = () => sut.Handle(exception);

            handle.Should().Throw<DbUpdateException>();
        }

        private class DuplicateHandlingController : IsolateController
        {
            public DuplicateHandlingController() : base(DbMock)
            {
            }

            public void Handle(DbUpdateException exception)
            {
                HandleDbUpdateException(exception);
            }
        }
    }
}