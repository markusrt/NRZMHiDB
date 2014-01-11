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
        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            CreateMockData();
        }

        private static void CreateMockData()
        {
            var catalog = new Catalog();

            for (var i = 1; i < EntityCount; i++)
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
    }
}