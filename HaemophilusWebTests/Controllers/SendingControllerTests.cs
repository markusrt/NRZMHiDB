using System;
using System.Collections;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
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
        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        readonly Catalog catalog = new Catalog();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            CreateMockData();
        }

        private void CreateMockData()
        {
            for (var i = FirstSendingId; i < EntityCount; i++)
            {
                var patient = catalog.CreateInstance<Patient>();
                patient.PatientId = i;
                DbMock.PatientDbSet.Add(patient);

                var sender = catalog.CreateInstance<Sender>();
                sender.SenderId = i;
                DbMock.Senders.Add(sender);

                var sending = catalog.CreateInstance<Sending>();
                sending.PatientId = i;
                sending.SenderId = i;
                sending.SendingId = i;
                sending.Isolate = null;
                DbMock.Sendings.Add(sending);
            }
        }

        private static SendingController CreateController()
        {
            return new SendingController(DbMock);
        }

        [Test]
        public void Index_ReturnsAllSendings()
        {
            var controller = CreateController();

            var result = controller.Index() as ViewResult;

            result.Should().NotBeNull();
            CollectionAssert.AreEquivalent((IEnumerable) result.Model, DbMock.Sendings);
        }

        [Test]
        public void Create_AddsPossiblePatientsAndSenders()
        {
            var controller = CreateController();

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();

            CollectionAssert.AreEquivalent(result.ViewBag.PossibleSenders, DbMock.Senders);
            CollectionAssert.AreEquivalent(result.ViewBag.PossiblePatients, DbMock.Patients);
        }

        [Test]
        public void Create_AddsOtherMaterials()
        {
            var availableOtherMaterials = DbMock.Sendings.Select(s => s.OtherMaterial).Distinct().ToList();
            var controller = CreateController();

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();

            CollectionAssert.AreEquivalent(result.ViewBag.PossibleOtherMaterials, 
                availableOtherMaterials);
        }

        [Test]
        public void AssignStemNumber_NoIsolate_AssignsCorrectLaboratoryNumber()
        {
            var expectedLaboratoryNumber = string.Format("1/{0:yy}", DateTime.Now);
            var controller = CreateController();

            var result = controller.AssignStemNumber(FirstSendingId) as JsonResult;
            var isolate = result.ConvertTo<Isolate>();

            isolate.Should().NotBeNull();
            isolate.LaboratoryNumber.Should().Be(expectedLaboratoryNumber);
        }

        [Test]
        public void AssignStemNumber_NoId_ReturnsError()
        {
            var controller = CreateController();

            var result = controller.AssignStemNumber(null) as JsonResult;
            var error = result.ConvertTo<Error>();

            error.ErrorMessage.Should().Contain("Sendungsnummer benötigt");
        }

        [Test]
        public void AssignStemNumber_UnknownId_ReturnsError()
        {
            const int unknownId = 12345;
            var controller = CreateController();

            var result = controller.AssignStemNumber(unknownId) as JsonResult;
            var error = result.ConvertTo<Error>();

            error.ErrorMessage.Should().Contain(unknownId.ToString());
            error.ErrorMessage.Should().Contain("existiert nicht");
        }

        [Test]
        public void AssignStemNumber_ExistingIsolate_DoesNotRecreateStemNumber()
        {
            var controller = CreateController();
            var viewResult = controller.Details(SecondSendingId) as ViewResult;
            var sending = viewResult.Model as Sending;
            sending.Isolate = catalog.CreateInstance<Isolate>();
            sending.Isolate.Year = 2099;
            sending.Isolate.YearlySequentialIsolateNumber = 999;

            var result = controller.AssignStemNumber(SecondSendingId) as JsonResult;
            var isolate = result.ConvertTo<Isolate>();

            isolate.Year.Should().Be(2099);
            isolate.YearlySequentialIsolateNumber.Should().Be(999);
        }
    }
}