using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    [TestFixture]
    public class SendingControllerTests
    {
        private ApplicationDbContextMock DbMock;

        internal SendingController CreateMockSendingController()
        {
            DbMock = new ApplicationDbContextMock();
            MockData.CreateMockData(DbMock);
            return new SendingController(DbMock);
        }

        [Test]
        public void AssignStemNumber_ExistingIsolate_DoesNotRecreateStemNumber()
        {
            var controller = CreateMockSendingController();
            var sending = DbMock.Sendings.Single(s => s.SendingId == MockData.SecondId);
            sending.Isolate = MockData.CreateInstance<Isolate>();
            sending.Isolate.Year = 2099;
            sending.Isolate.YearlySequentialIsolateNumber = 999;

            controller.CreateSendingAndAssignStemAndLaboratoryNumber(sending);

            sending.Isolate.Year.Should().Be(2099);
            sending.Isolate.YearlySequentialIsolateNumber.Should().Be(999);
        }

        [Test]
        public void AssignStemNumber_NoId_ReturnsError()
        {
            var controller = CreateMockSendingController();

            Assert.Throws<ArgumentException>(() => controller.AssignStemAndLaboratoryNumber(null));
        }

        [Test]
        public void AssignStemNumber_NoIsolate_AssignsCorrectLaboratoryNumber()
        {
            var expectedLaboratoryNumber = $"001/{DateTime.Now:yy}";
            var controller = CreateMockSendingController();
            var sending = DbMock.Sendings.Single(s => s.SendingId == MockData.FirstId);

            controller.CreateSendingAndAssignStemAndLaboratoryNumber(sending);

            var isolate = sending.Isolate;
            isolate.Should().NotBeNull();
            isolate.LaboratoryNumber.Should().Be(expectedLaboratoryNumber);
            isolate.StemNumber.Should().BeGreaterThan(0);
        }

        [Test]
        public void AssignStemNumber_SendingWithoutAutoAssignStemNumber_AssignsCorrectLaboratoryNumberButNoStemNumber()
        {
            var expectedLaboratoryNumber = $"001/{DateTime.Now:yy}";
            var controller = CreateMockSendingController();
            var sending = MockData.CreateInstance<NoAutoAssignStemNumberSending>();
            sending.SendingId = 123;
            sending.SenderId = MockData.FirstId;
            sending.PatientId = MockData.FirstId;
            sending.Isolate = null;

            controller.CreateSendingAndAssignStemAndLaboratoryNumber(sending);

            var isolate = sending.Isolate;
            isolate.Should().NotBeNull();
            isolate.LaboratoryNumber.Should().Be(expectedLaboratoryNumber);
            isolate.StemNumber.HasValue.Should().BeFalse();
        }

        [Test]
        public void AssignStemNumber_UnknownId_ReturnsError()
        {
            const int unknownId = 12345;
            var controller = CreateMockSendingController();

            Assert.Throws<ArgumentException>(() => controller.AssignStemAndLaboratoryNumber(unknownId));
        }

        [Test]
        public void Create_AddsOtherSamplingLocations()
        {
            var controller = CreateMockSendingController();
            var availableOtherSamplingLocations =
                DbMock.Sendings.Select(s => s.OtherSamplingLocation).Distinct().ToList();

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();

            CollectionAssert.AreEquivalent(result.ViewBag.PossibleOtherSamplingLocations,
                availableOtherSamplingLocations);
        }

        [Test]
        public void Create_NewSending_SuggestsLaboratoryNumber()
        {
            var controller = CreateMockSendingController();

            var result = controller.Create() as ViewResult;

            result.Model.As<Sending>().LaboratoryNumber.Should().StartWith("001/");
        }

        [Test]
        public void Edit_SendingWithIsolate_RedirectsLaboratoryNumber()
        {
            var controller = CreateMockSendingController();
            var sending = DbMock.Sendings.Single(s => s.SendingId == MockData.ThirdId);
            sending.Isolate = new Isolate { YearlySequentialIsolateNumber = 14, Year = 2050 };

            var result = controller.Edit(sending.SendingId) as ViewResult;

            result.Model.As<Sending>().LaboratoryNumber.Should().Be("014/50");
        }

        [Test]
        public void Create_AddsPossiblePatientsAndNonDeletedSenders()
        {
            var controller = CreateMockSendingController();
            DbMock.Senders.First().Deleted = true;

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();

            CollectionAssert.AreEquivalent(result.ViewBag.PossibleSenders, DbMock.Senders.Where(s => !s.Deleted));
            CollectionAssert.AreEquivalent(result.ViewBag.PossiblePatients, DbMock.Patients);
        }

        [Test]
        public void Index_ReturnsOnlyNonDeletedSendings()
        {
            var controller = CreateMockSendingController();

            var result = controller.Index() as ViewResult;

            result.Should().NotBeNull();
            CollectionAssert.AreEquivalent((IEnumerable)result.Model, DbMock.Sendings.Where(s => !s.Deleted));
        }

        class NoAutoAssignStemNumberSending : Sending
        {
            public override bool AutoAssignStemNumber => false;
        }
    }
}