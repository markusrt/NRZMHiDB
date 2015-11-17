using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    [TestFixture]
    public class SendingControllerTests
    {
        private const int EntityCount = 11;
        internal const int FirstId = 1;
        private const int SecondId = 2;
        private const int ThirdId = 3;
        internal static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        private static readonly Catalog Catalog = new Catalog();

        static SendingControllerTests()
        {
            CreateMockData();
        }

        private static void CreateMockData()
        {
            for (var i = FirstId; i < EntityCount; i++)
            {
                var patient = Catalog.CreateInstance<Patient>();
                patient.PatientId = i;
                DbMock.PatientDbSet.Add(patient);

                var sender = Catalog.CreateInstance<Sender>();
                sender.SenderId = i;
                DbMock.Senders.Add(sender);

                var sending = Catalog.CreateInstance<Sending>();
                sending.PatientId = i;
                sending.SenderId = i;
                sending.SendingId = i;
                sending.Isolate = null;
                DbMock.Sendings.Add(sending);
            }
        }

        internal static SendingController CreateMockSendingController()
        {
            return new SendingController(DbMock);
        }

        [Test]
        public void AssignStemNumber_ExistingIsolate_DoesNotRecreateStemNumber()
        {
            var controller = CreateMockSendingController();
            var sending = DbMock.Sendings.Single(s => s.SendingId == SecondId);
            sending.Isolate = Catalog.CreateInstance<Isolate>();
            sending.Isolate.Year = 2099;
            sending.Isolate.YearlySequentialIsolateNumber = 999;

            var isolate = controller.AssignStemNumber(SecondId);

            isolate.Year.Should().Be(2099);
            isolate.YearlySequentialIsolateNumber.Should().Be(999);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void AssignStemNumber_NoId_ReturnsError()
        {
            var controller = CreateMockSendingController();

            controller.AssignStemNumber(null);
        }

        [Test]
        public void AssignStemNumber_NoIsolate_AssignsCorrectLaboratoryNumber()
        {
            var expectedLaboratoryNumber = string.Format("001/{0:yy}", DateTime.Now);
            var controller = CreateMockSendingController();

            var isolate = controller.AssignStemNumber(FirstId);

            isolate.Should().NotBeNull();
            isolate.LaboratoryNumber.Should().Be(expectedLaboratoryNumber);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void AssignStemNumber_UnknownId_ReturnsError()
        {
            const int unknownId = 12345;
            var controller = CreateMockSendingController();

            controller.AssignStemNumber(unknownId);
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
            var sending = DbMock.Sendings.Single(s => s.SendingId == ThirdId);
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