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
        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        static SendingControllerTests()
        {
            MockData.CreateMockData(DbMock);
        }

        internal static SendingController CreateMockSendingController()
        {
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

            var isolate = controller.AssignStemNumber(MockData.SecondId);

            isolate.Year.Should().Be(2099);
            isolate.YearlySequentialIsolateNumber.Should().Be(999);
        }

        [Test]
        public void AssignStemNumber_NoId_ReturnsError()
        {
            var controller = CreateMockSendingController();

            Assert.Throws<ArgumentException>(() => controller.AssignStemNumber(null));
        }

        [Test]
        public void AssignStemNumber_NoIsolate_AssignsCorrectLaboratoryNumber()
        {
            var expectedLaboratoryNumber = string.Format("001/{0:yy}", DateTime.Now);
            var controller = CreateMockSendingController();

            var isolate = controller.AssignStemNumber(MockData.FirstId);

            isolate.Should().NotBeNull();
            isolate.LaboratoryNumber.Should().Be(expectedLaboratoryNumber);
        }

        [Test]
        public void AssignStemNumber_UnknownId_ReturnsError()
        {
            const int unknownId = 12345;
            var controller = CreateMockSendingController();

            Assert.Throws<ArgumentException>(() => controller.AssignStemNumber(unknownId));
        }

        [Test]
        public void Create_AddsOtherSamplingLocations()
        {
            var availableOtherSamplingLocations =
                DbMock.Sendings.Select(s => s.OtherSamplingLocation).Distinct().ToList();
            var controller = CreateMockSendingController();

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
            sending.Isolate = new Isolate {YearlySequentialIsolateNumber = 14, Year = 2050};

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
            CollectionAssert.AreEquivalent((IEnumerable) result.Model, DbMock.Sendings.Where(s => !s.Deleted));
        }
    }
}