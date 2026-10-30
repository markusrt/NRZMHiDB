using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataTables.Mvc;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class MeningoPatientSendingControllerTests
    {
        private const int SenderId = 7;

        // specs/003 FR-002
        [Test]
        public void Create_NewPatientSending_PrefillsDefaults()
        {
            var controller = CreateController(new IntakeDbStub());

            var model = controller.Create().As<ViewResult>().Model.As<PatientSendingViewModel<MeningoPatient, MeningoSending>>();

            model.Sending.ReceivingDate.Date.Should().Be(DateTime.Today);
            model.Sending.SamplingDate.Should().HaveValue();
            model.Sending.SamplingDate.Value.Date.Should().Be(DateTime.Today.AddDays(-7));
            model.Sending.SamplingLocation.Should().Be(MeningoSamplingLocation.Blood);
            model.Sending.Material.Should().Be(MeningoMaterial.VitalStem);
            model.Sending.SenderSpecies.Should().Be("N. meningitidis");
            model.Patient.State.Should().Be(State.Unknown);
        }

        // specs/003 FR-003: laboratory number of the current year, no stem number
        [Test]
        public void Create_ValidModel_LinksSenderAndPatientAndAssignsLaboratoryNumberButNoStemNumber()
        {
            var db = new IntakeDbStub();
            var controller = CreateController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.Sending.SamplingDate = new DateTime(2020, 3, 10);
            intake.Sending.ReceivingDate = new DateTime(2020, 3, 12);

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
            var patient = db.MeningoPatients.Should().ContainSingle().Subject;
            var sending = db.MeningoSendings.Should().ContainSingle().Subject;
            sending.SenderId.Should().Be(SenderId);
            sending.MeningoPatientId.Should().Be(patient.PatientId);
            sending.Isolate.Should().NotBeNull();
            sending.Isolate.LaboratoryNumber.Should().Be($"001/{DateTime.Now:yy}");
            sending.Isolate.StemNumber.Should().BeNull();
        }

        // specs/003 FR-008
        [Test]
        public void Create_SamplingDateBeforeBirthDate_RejectsAndStoresNothing()
        {
            var db = new IntakeDbStub();
            var controller = CreateController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.Sending.SamplingDate = intake.Patient.BirthDate.Value.AddDays(-1);

            var result = controller.Create(intake);

            result.Should().BeOfType<ViewResult>();
            controller.ModelState["Sending.SamplingDate"].Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Das Entnahmedatum muss nach dem Geburtsdatum des Patienten liegen");
            db.MeningoPatients.Should().BeEmpty();
            db.MeningoSendings.Should().BeEmpty();
        }

        // specs/003 FR-008: the rule is skipped when either date is missing
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Create_SamplingDateOrBirthDateMissing_DoesNotCheckDateOrder(bool withoutSamplingDate, bool withoutBirthDate)
        {
            var controller = CreateController(new IntakeDbStub());
            var intake = CreateIntake(100, "X.A.");
            if (withoutSamplingDate)
            {
                intake.Sending.SamplingDate = null;
            }
            if (withoutBirthDate)
            {
                intake.Patient.BirthDate = null;
            }

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
        }

        // specs/003 FR-010, Key Entities (initials stored upper-cased)
        [Test]
        public void Create_LowerCaseInitialsOfOneExistingPatient_PausesForDuplicateResolution()
        {
            var db = new IntakeDbStub();
            db.MeningoPatients.Add(CreatePatient(42, "X.A."));
            var controller = CreateController(db);

            var result = controller.Create(CreateIntake(100, "x.a."));

            var model = result.Should().BeOfType<ViewResult>().Subject.Model.As<PatientSendingViewModel<MeningoPatient, MeningoSending>>();
            model.DuplicatePatientDetected.Should().BeTrue();
            db.MeningoPatients.Should().ContainSingle();
            db.MeningoSendings.Should().BeEmpty();
        }

        // specs/003 FR-011
        [Test]
        public void Create_UseExistingPatient_AttachesSendingToExistingPatientAndUpdatesIt()
        {
            var db = new IntakeDbStub();
            db.MeningoPatients.Add(CreatePatient(42, "X.A."));
            var controller = CreateController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.Patient.City = "SV-003-Ort";
            intake.DuplicatePatientResolution = DuplicatePatientResolution.UseExistingPatient;

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
            db.MeningoPatients.Should().ContainSingle();
            db.ModifiedEntities.OfType<MeningoPatient>().Should().ContainSingle()
                .Which.Should().Match<MeningoPatient>(p => p.PatientId == 42 && p.City == "SV-003-Ort");
            db.MeningoSendings.Should().ContainSingle().Which.MeningoPatientId.Should().Be(42);
        }

        // specs/003 FR-011
        [Test]
        public void Create_CreateNewPatient_CreatesSecondPatientWithSameIdentifyingDetails()
        {
            var db = new IntakeDbStub();
            db.MeningoPatients.Add(CreatePatient(42, "X.A."));
            var controller = CreateController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.DuplicatePatientResolution = DuplicatePatientResolution.CreateNewPatient;

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
            db.MeningoPatients.Should().HaveCount(2);
            db.MeningoSendings.Should().ContainSingle().Which.MeningoPatientId.Should().NotBe(42);
        }

        // specs/003 FR-012 (without a posted resolution; see BACKLOG DATA-031 for the bypass)
        [Test]
        public void Create_TwoExistingPatientsMatch_RejectsThirdPatient()
        {
            var db = new IntakeDbStub();
            db.MeningoPatients.Add(CreatePatient(42, "X.A."));
            db.MeningoPatients.Add(CreatePatient(43, "X.A."));
            var controller = CreateController(db);

            var result = controller.Create(CreateIntake(100, "X.A."));

            result.Should().BeOfType<ViewResult>();
            controller.ModelState[""].Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().StartWith("Es existieren bereits zwei Patienten");
            db.MeningoPatients.Should().HaveCount(2);
            db.MeningoSendings.Should().BeEmpty();
        }

        // specs/003 FR-013
        [Test]
        public void Create_LaboratoryNumberTakenMeanwhile_WarnsWithNewLaboratoryNumber()
        {
            var db = new IntakeDbStub();
            db.MeningoIsolates.Add(new MeningoIsolate { Year = DateTime.Now.Year, YearlySequentialIsolateNumber = 1 });
            var controller = CreateController(db);
            var intake = CreateIntake(100, "X.B.");
            intake.Sending.LaboratoryNumber = $"001/{DateTime.Now:yy}";

            controller.Create(intake);

            controller.TempData["WarningMessage"].Should().Be(
                $"Beim Speichern der letzten Einsendung hat sich die Labornummer geändert. Neue Labornummer: 002/{DateTime.Now:yy}");
        }

        // specs/003 FR-013
        [Test]
        public void Create_LaboratoryNumberAsShown_DoesNotWarn()
        {
            var controller = CreateController(new IntakeDbStub());
            var intake = CreateIntake(100, "X.B.");
            intake.Sending.LaboratoryNumber = $"001/{DateTime.Now:yy}";

            controller.Create(intake);

            controller.TempData.ContainsKey("WarningMessage").Should().BeFalse();
        }

        // specs/003 FR-004, FR-005 (one sender directory for both databases)
        [Test]
        public void Edit_SendingOfDeletedSender_OffersActiveSendersAndTheAttachedOne()
        {
            var db = new IntakeDbStub();
            db.Senders.Add(new Sender { SenderId = 1, Name = "Active" });
            db.Senders.Add(new Sender { SenderId = 2, Name = "Deleted, attached", Deleted = true });
            db.Senders.Add(new Sender { SenderId = 3, Name = "Deleted, unrelated", Deleted = true });
            var sending = CreateStoredSending(100, 2, CreatePatient(42, "X.A."));
            db.MeningoPatients.Add(sending.Patient);
            db.MeningoSendings.Add(sending);

            var edit = CreateController(db).Edit(100).As<ViewResult>();
            var create = CreateController(db).Create().As<ViewResult>();

            ((IEnumerable<Sender>) edit.ViewBag.PossibleSenders).Select(s => s.SenderId).Should().BeEquivalentTo(new[] { 1, 2 });
            ((IEnumerable<Sender>) create.ViewBag.PossibleSenders).Select(s => s.SenderId).Should().BeEquivalentTo(new[] { 1 });
        }

        // specs/003 FR-017
        [Test]
        public void Edit_ValidModel_SavesPatientAndSendingAndRedirectsToList()
        {
            var db = new IntakeDbStub();
            var controller = CreateController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.Patient.PatientId = 42;
            intake.Sending.MeningoPatientId = 42;

            var result = controller.Edit(intake);

            result.Should().BeOfType<RedirectToRouteResult>().Which.RouteValues["action"].Should().Be("Index");
            db.ModifiedEntities.Should().BeEquivalentTo(new object[] { intake.Patient, intake.Sending });
            db.SavedWithoutValidation.Should().Equal(false);
        }

        // specs/003 FR-018, FR-019, US3.3 (request sent outside the normal pages)
        [Test]
        public void DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation()
        {
            var db = new IntakeDbStub();
            var sending = new MeningoSending { MeningoSendingId = 100, SenderLaboratoryNumber = null, SenderSpecies = null, Patient = CreatePatient(42, null) };
            db.MeningoSendings.Add(sending);
            var controller = CreateController(db);

            var result = controller.DeleteConfirmed(100);

            result.Should().BeOfType<RedirectToRouteResult>().Which.RouteValues["action"].Should().Be("Index");
            sending.Deleted.Should().BeTrue();
            db.SavedWithoutValidation.Should().Equal(true);
        }

        // specs/003 FR-019, US3.4
        [Test]
        public void Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation()
        {
            var db = new IntakeDbStub();
            var sending = new MeningoSending { MeningoSendingId = 100, Deleted = true, SenderLaboratoryNumber = null, SenderSpecies = null, Patient = CreatePatient(42, null) };
            db.MeningoSendings.Add(sending);
            var controller = CreateController(db);

            var result = controller.Undelete(100);

            result.Should().BeOfType<RedirectToRouteResult>().Which.RouteValues["action"].Should().Be("Index");
            sending.Deleted.Should().BeFalse();
            db.SavedWithoutValidation.Should().Equal(true);
        }

        // specs/003 FR-014, FR-015, FR-018
        [Test]
        public void DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns()
        {
            var db = new IntakeDbStub();
            db.Senders.Add(new Sender { SenderId = SenderId, Name = "SV-003", PostalCode = "97080" });
            var active = CreateStoredSending(100, SenderId, CreatePatient(42, "X.A."));
            active.SenderLaboratoryNumber = "SV-003-A1";
            active.SamplingLocation = MeningoSamplingLocation.PharynxSwab;
            var deleted = CreateStoredSending(101, SenderId, CreatePatient(43, "X.B."));
            deleted.Deleted = true;
            db.MeningoSendings.Add(active);
            db.MeningoSendings.Add(deleted);
            var controller = CreateController(db);
            var request = new DefaultDataTablesRequest { Search = new Search("", false), Columns = new ColumnCollection(new List<Column>()), Length = 10 };

            var response = (DataTablesResponse) controller.DataTableAjax(request).Data;

            response.recordsTotal.Should().Be(1);
            var row = response.data.Cast<dynamic>().Single();
            ((string) row.Initials).Should().Be("X.A.");
            ((string) row.PatientPostalCode).Should().Be("97082");
            ((string) row.SenderPostalCode).Should().Be("97080");
            ((string) row.SenderLaboratoryNumber).Should().Be("SV-003-A1");
            ((int) row.PatientId).Should().Be(42);
            ((string) row.SamplingLocation).Should().Be("Rachenabstrich");
            ((string) row.Invasive).Should().Be("Nein");
        }

        private static MeningoPatient CreatePatient(int patientId, string initials)
        {
            return new MeningoPatient
            {
                PatientId = patientId,
                Initials = initials,
                BirthDate = new DateTime(2001, 2, 1),
                PostalCode = "97082",
                Gender = Gender.Female,
                State = State.BY
            };
        }

        private static PatientSendingViewModel<MeningoPatient, MeningoSending> CreateIntake(int sendingId, string initials)
        {
            return new PatientSendingViewModel<MeningoPatient, MeningoSending>
            {
                Patient = CreatePatient(0, initials),
                Sending = new MeningoSending
                {
                    MeningoSendingId = sendingId,
                    SenderId = SenderId,
                    SenderLaboratoryNumber = "SV-003"
                }
            };
        }

        private static MeningoSending CreateStoredSending(int sendingId, int senderId, MeningoPatient patient)
        {
            return new MeningoSending
            {
                MeningoSendingId = sendingId,
                SenderId = senderId,
                Patient = patient,
                MeningoPatientId = patient.PatientId,
                SenderLaboratoryNumber = "SV-003",
                Isolate = new MeningoIsolate { MeningoIsolateId = sendingId, Year = 2026, YearlySequentialIsolateNumber = sendingId }
            };
        }

        private static MeningoPatientSendingController CreateController(IntakeDbStub db)
        {
            var geonamesController = new GeonamesController(new GeonamesService(), path => "{\"geonames\":[]}");
            var controller = new MeningoPatientSendingController(db, new MeningoPatientController(db, geonamesController), new MeningoSendingController(db));
            var request = Substitute.For<HttpRequestBase>();
            request.Form.Returns(new NameValueCollection());
            var context = Substitute.For<HttpContextBase>();
            context.Request.Returns(request);
            context.Server.Returns(Substitute.For<HttpServerUtilityBase>());
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            controller.Url = Substitute.For<UrlHelper>();
            return controller;
        }

        /// <summary>
        ///     Context with its own (non-static) sets, so these tests do not share data with
        ///     <see cref="ApplicationDbContextMock" />; records modified entities and whether SaveChanges ran without
        ///     save validation.
        /// </summary>
        private class IntakeDbStub : IApplicationDbContext
        {
            private bool validationDisabled;

            public readonly List<bool> SavedWithoutValidation = new List<bool>();

            public readonly List<object> ModifiedEntities = new List<object>();

            public IDbSet<Sender> Senders { get; } = new InMemoryDbSet<Sender>(new HashSet<Sender>());
            public IDbSet<Patient> Patients { get; } = new InMemoryDbSet<Patient>(new HashSet<Patient>());
            public IDbSet<Sending> Sendings { get; } = new InMemoryDbSet<Sending>(new HashSet<Sending>());
            public IDbSet<Isolate> Isolates { get; } = new InMemoryDbSet<Isolate>(new HashSet<Isolate>());
            public IDbSet<County> Counties { get; } = new InMemoryDbSet<County>(new HashSet<County>());
            public IDbSet<HealthOffice> HealthOffices { get; } = new InMemoryDbSet<HealthOffice>(new HashSet<HealthOffice>());
            public IDbSet<ConfigurationEntry> ConfigurationEntries { get; } = new InMemoryDbSet<ConfigurationEntry>(new HashSet<ConfigurationEntry>());
            public IDbSet<EpsilometerTest> EpsilometerTests { get; } = new InMemoryDbSet<EpsilometerTest>(new HashSet<EpsilometerTest>());
            public IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints { get; } = new InMemoryDbSet<EucastClinicalBreakpoint>(new HashSet<EucastClinicalBreakpoint>());

            public IDbSet<MeningoPatient> MeningoPatients { get; } = new InMemoryDbSet<MeningoPatient>(new HashSet<MeningoPatient>())
            {
                FindFunction = (patients, keys) => patients.FirstOrDefault(p => p.PatientId == (int) keys[0])
            };

            public IDbSet<MeningoSending> MeningoSendings { get; } = new InMemoryDbSet<MeningoSending>(new HashSet<MeningoSending>())
            {
                FindFunction = (sendings, keys) => sendings.FirstOrDefault(s => s.MeningoSendingId == (int) keys[0])
            };

            public IDbSet<MeningoIsolate> MeningoIsolates { get; } = new InMemoryDbSet<MeningoIsolate>(new HashSet<MeningoIsolate>());
            public IDbSet<NeisseriaPubMlstIsolate> NeisseriaPubMlstIsolates { get; } = new InMemoryDbSet<NeisseriaPubMlstIsolate>(new HashSet<NeisseriaPubMlstIsolate>());

            public void MarkAsModified<TEntity>(TEntity entity) where TEntity : class
            {
                ModifiedEntities.Add(entity);
            }

            public int SaveChanges()
            {
                SavedWithoutValidation.Add(validationDisabled);
                return 0;
            }

            public void WrapInTransaction(Action action)
            {
                action();
            }

            public void PerformWithoutSaveValidation(Action action)
            {
                validationDisabled = true;
                action();
                validationDisabled = false;
            }

            public void Dispose()
            {
            }
        }
    }
}
