using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.Utils;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    [TestFixture]
    public class SendingControllerTests
    {
        private const int EntityCount = 11;
        private const int FirstSendingId = 1;
        private const int SecondSendingId = 2;
        internal static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        private static readonly Catalog Catalog = new Catalog();

        static SendingControllerTests()
        {
            CreateMockData();
        }

        private static void CreateMockData()
        {
            for (var i = FirstSendingId; i < EntityCount; i++)
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
            var viewResult = controller.Details(SecondSendingId) as ViewResult;
            var sending = viewResult.Model as Sending;
            sending.Isolate = Catalog.CreateInstance<Isolate>();
            sending.Isolate.Year = 2099;
            sending.Isolate.YearlySequentialIsolateNumber = 999;

            var isolate = controller.AssignStemNumber(SecondSendingId);

            isolate.Year.Should().Be(2099);
            isolate.YearlySequentialIsolateNumber.Should().Be(999);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AssignStemNumber_NoId_ReturnsError()
        {
            var controller = CreateMockSendingController();

            controller.AssignStemNumber(null);
        }

        [Test]
        public void AssignStemNumber_NoIsolate_AssignsCorrectLaboratoryNumber()
        {
            var expectedLaboratoryNumber = string.Format("1/{0:yy}", DateTime.Now);
            var controller = CreateMockSendingController();

            var isolate = controller.AssignStemNumber(FirstSendingId);

            isolate.Should().NotBeNull();
            isolate.LaboratoryNumber.Should().Be(expectedLaboratoryNumber);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AssignStemNumber_UnknownId_ReturnsError()
        {
            const int unknownId = 12345;
            var controller = CreateMockSendingController();

            controller.AssignStemNumber(unknownId);
        }

        [Test]
        public void Create_AddsOtherMaterials()
        {
            var availableOtherMaterials = DbMock.Sendings.Select(s => s.OtherMaterial).Distinct().ToList();
            var controller = CreateMockSendingController();

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();

            CollectionAssert.AreEquivalent(result.ViewBag.PossibleOtherMaterials,
                availableOtherMaterials);
        }

        [Test]
        public void Create_AddsPossiblePatientsAndSenders()
        {
            var controller = CreateMockSendingController();

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();

            CollectionAssert.AreEquivalent(result.ViewBag.PossibleSenders, DbMock.Senders);
            CollectionAssert.AreEquivalent(result.ViewBag.PossiblePatients, DbMock.Patients);
        }

        [Test]
        public void Index_ReturnsAllSendings()
        {
            var controller = CreateMockSendingController();

            var result = controller.Index() as ViewResult;

            result.Should().NotBeNull();
            CollectionAssert.AreEquivalent((IEnumerable) result.Model, DbMock.Sendings);
        }
    }
}