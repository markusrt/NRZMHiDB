using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Automapper;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    // specs/009: Meningococci report controller. Own data sets instead of the shared static store of
    // ApplicationDbContextMock, so these tests neither depend on nor disturb other fixtures.
    public class MeningoReportControllerTests
    {
        private const int IsolateId = 20;
        private const int SenderId = 7;
        private const string MeningoTemplatesPath = "~/ReportTemplates/Meningo";

        private IApplicationDbContext db;
        private IApplicationDbContext previousMappingDb;
        private HttpServerUtilityBase server;
        private HashSet<Sender> senders;
        private HashSet<ConfigurationEntry> configurationEntries;
        private MeningoIsolate isolate;
        private MeningoReportController controller;

        [OneTimeSetUp]
        public void InitializeAutomapper()
        {
            MvcApplication.Mapper.Should().NotBeNull();
        }

        [SetUp]
        public void SetUp()
        {
            var penicillin = new EucastClinicalBreakpoint
            {
                EucastClinicalBreakpointId = 1, Antibiotic = Antibiotic.Benzylpenicillin, ValidFor = DatabaseType.Meningococci,
                MicBreakpointSusceptible = 0.064f, MicBreakpointResistent = 0.25f, ValidFrom = new DateTime(2024, 1, 1)
            };
            var azithromycin = new EucastClinicalBreakpoint
            {
                EucastClinicalBreakpointId = 2, Antibiotic = Antibiotic.Azithromycin, ValidFor = DatabaseType.Meningococci
            };

            isolate = new MeningoIsolate
            {
                MeningoIsolateId = IsolateId,
                YearlySequentialIsolateNumber = 11,
                Year = 2026,
                EpsilometerTests = new List<EpsilometerTest>
                {
                    new EpsilometerTest { EucastClinicalBreakpointId = 1, EucastClinicalBreakpoint = penicillin, Measurement = 0.064f, Result = EpsilometerTestResult.Susceptible },
                    new EpsilometerTest { EucastClinicalBreakpointId = 2, EucastClinicalBreakpoint = azithromycin, Measurement = 1f, Result = EpsilometerTestResult.NotDetermined }
                },
                Sending = new MeningoSending
                {
                    SenderId = SenderId,
                    SamplingDate = new DateTime(2026, 11, 1),
                    ReceivingDate = new DateTime(2026, 11, 8),
                    SamplingLocation = MeningoSamplingLocation.Blood,
                    Material = MeningoMaterial.VitalStem,
                    Patient = new MeningoPatient { PatientId = 3, Initials = "T.T.", BirthDate = new DateTime(2000, 1, 1), PostalCode = "97080" }
                }
            };

            senders = new HashSet<Sender> { new Sender { SenderId = SenderId, Name = "SV-009 Sender", PostalCode = "97080", City = "Würzburg", StreetWithNumber = "Teststraße 1" } };
            configurationEntries = new HashSet<ConfigurationEntry>();

            db = Substitute.For<IApplicationDbContext>();
            db.MeningoIsolates.Returns(new InMemoryDbSet<MeningoIsolate>(new HashSet<MeningoIsolate> { isolate }));
            db.Senders.Returns(new InMemoryDbSet<Sender>(senders)
            {
                FindFunction = (all, keys) => all.FirstOrDefault(s => s.SenderId == (int) keys[0])
            });
            db.EucastClinicalBreakpoints.Returns(new InMemoryDbSet<EucastClinicalBreakpoint>(new HashSet<EucastClinicalBreakpoint> { penicillin, azithromycin }));
            db.ConfigurationEntries.Returns(new InMemoryDbSet<ConfigurationEntry>(configurationEntries));

            previousMappingDb = IsolateViewModelMappingActionBase.DbForTest;
            IsolateViewModelMappingActionBase.DbForTest = db;

            controller = new MeningoReportController(db);
            var context = Substitute.For<HttpContextBase>();
            server = Substitute.For<HttpServerUtilityBase>();
            server.MapPath(Arg.Any<string>()).Returns(Path.Combine(Path.GetTempPath(), "SV-009-no-templates-" + Guid.NewGuid()));
            context.Server.Returns(server);
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
        }

        [TearDown]
        public void TearDown()
        {
            IsolateViewModelMappingActionBase.DbForTest = previousMappingDb;
        }

        // specs/009 FR-003, Edge Cases (missing identifier)
        [Test]
        public void Isolate_WithoutId_ReturnsBadRequest()
        {
            var result = controller.Isolate(null) as HttpStatusCodeResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        // specs/009 FR-003, Edge Cases (unknown identifier)
        [Test]
        public void Isolate_UnknownId_ReturnsNotFound()
        {
            var result = controller.Isolate(4711);

            result.Should().BeOfType<HttpNotFoundResult>();
        }

        // specs/009 FR-006, US3.2, SC-006
        [Test]
        public void Isolate_MeasuredAzithromycin_IsRemovedWhileOtherMeasuredResultsRemain()
        {
            var viewResult = (ViewResult) controller.Isolate(IsolateId);

            var model = (MeningoIsolateViewModel) viewResult.Model;
            model.EpsilometerTestViewModels.Should().NotContain(e => e.Antibiotic == Antibiotic.Azithromycin);
            model.EpsilometerTestViewModels.Should().Contain(e => e.Antibiotic == Antibiotic.Benzylpenicillin && e.Measurement == 0.064f);
            model.ETests.Should().ContainSingle();
        }

        // specs/009 FR-007: the Meningococci template folder and the configured signers are offered
        [Test]
        public void Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners()
        {
            configurationEntries.Add(new ConfigurationEntry { Key = ConfigurationService.Keys.ReportSigners, Value = "[\"Signer One\",\"Signer Two\"]" });

            var viewResult = (ViewResult) controller.Isolate(IsolateId);

            server.Received().MapPath(MeningoTemplatesPath);
            ((List<string>) viewResult.ViewBag.ReportSigners).Should().Equal("Signer One", "Signer Two");
            ((List<FileInfo>) viewResult.ViewBag.ReportTemplates).Should().BeEmpty();
        }

        // specs/009 Edge Cases: an unresolvable sender leaves the sender address empty, report creation continues
        [Test]
        public void Isolate_SenderNotFound_LeavesSenderFieldsEmptyAndShowsReportPage()
        {
            senders.Clear();

            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            viewResult.Should().NotBeNull();
            var model = (MeningoIsolateViewModel) viewResult.Model;
            model.SenderName.Should().BeNull();
            model.SenderStreet.Should().BeNull();
            model.SenderCity.Should().BeNull();
            model.Patient.Should().NotBeNullOrEmpty();
        }

        // specs/009 FR-017, FR-018, FR-019, Report State Transitions
        [TestCase(ReportStatus.None, true, ReportStatus.Preliminary)]
        [TestCase(ReportStatus.Preliminary, true, ReportStatus.Preliminary)]
        [TestCase(ReportStatus.Final, true, ReportStatus.Final)]
        [TestCase(ReportStatus.None, false, ReportStatus.Final)]
        [TestCase(ReportStatus.Preliminary, false, ReportStatus.Final)]
        [TestCase(ReportStatus.Final, false, ReportStatus.Final)]
        public void ReportGenerated_StartingState_EndsInDocumentedStateAndReportDate(ReportStatus startingState, bool preliminary, ReportStatus expectedState)
        {
            var previousReportDate = startingState == ReportStatus.Final ? new DateTime(2026, 1, 2, 3, 4, 5) : (DateTime?) null;
            isolate.ReportStatus = startingState;
            isolate.ReportDate = previousReportDate;

            var result = controller.ReportGenerated(IsolateId, preliminary);

            result.Data.Should().Be(true);
            isolate.ReportStatus.Should().Be(expectedState);
            if (preliminary)
            {
                isolate.ReportDate.Should().Be(previousReportDate);
            }
            else
            {
                isolate.ReportDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            }
        }

        // specs/009 FR-020, Edge Cases (unknown or missing identifier in the notification)
        [TestCase(4711, false)]
        [TestCase(4711, true)]
        [TestCase(null, false)]
        [TestCase(null, true)]
        public void ReportGenerated_UnknownOrMissingIsolateId_AcknowledgesAndChangesNoIsolate(int? isolateId, bool preliminary)
        {
            var result = controller.ReportGenerated(isolateId, preliminary);

            result.Data.Should().Be(true);
            isolate.ReportStatus.Should().Be(ReportStatus.None);
            isolate.ReportDate.Should().BeNull();
        }
    }
}
