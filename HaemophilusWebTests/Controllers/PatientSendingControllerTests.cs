using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataTables.Mvc;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingControllerTests : ITempDirectoryTest
    {
        private SendingController sendingController;
        private PatientController patientController;
        private ApplicationDbContextMock DbMock;
        private NameValueCollection requestForm = new NameValueCollection();

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            MockData.CreateMockData(DbMock);
            patientController = new PatientController(DbMock);
            sendingController = new SendingController(DbMock);
            foreach (var sending in DbMock.Sendings)
            {
                sending.Isolate = MockData.CreateInstance<Isolate>();
                sending.Isolate.EpsilometerTests.Clear();
                sending.Isolate.EpsilometerTests.Add(MockData.CreateInstance<EpsilometerTest>());
            }
        }

        [Test]
        public void Create_NewPatientSending_ViewDataContainsSameDataAsIfUsingSendingController()
        {
            var controller = CreateController();
            var sendingResults = sendingController.Create().As<ViewResult>().ViewData.Keys;
            var patientResults = patientController.Create().As<ViewResult>().ViewData.Keys;
            var expectedResults = sendingResults.Union(patientResults);

            var actualResult = controller.Create().As<ViewResult>().ViewData.Keys;

            actualResult.Should().BeEquivalentTo(expectedResults);
        }

        [Test]
        public void Create_InvalidModel_ProducesValidationErrors()
        {
            var controller = CreateController();

            controller.Create(new PatientSendingViewModel<Patient,Sending> {Patient = new Patient(), Sending = new Sending()});

            controller.ModelState.Should().NotBeEmpty();
        }

        [Test]
        public void Create_AssignsClinicalInformation()
        {
            var controller = CreateController();
            requestForm.Set("ClinicalInformation", "Pneumonia,Sepsis");

            var patient = MockData.CreateInstance<Patient>();
            controller.Create(new PatientSendingViewModel<Patient, Sending> { Patient = patient, Sending = MockData.CreateInstance<Sending>()});

            var patientInDatabase = DbMock.Patients.Last();
            patientInDatabase.Initials.Should().Be(patient.Initials);
            patientInDatabase.ClinicalInformation.Should().Be(ClinicalInformation.Sepsis|ClinicalInformation.Pneumonia);
        }


        [TestCase(ReportStatus.None, "glyphicon-remove-sign")]
        [TestCase(ReportStatus.Preliminary, "glyphicon-time")]
        [TestCase(ReportStatus.Final, "glyphicon-ok-sign")]
        public void DataTableAjax_ReportGenerated_IsSetAccordingToReportState(ReportStatus reportStatus, string expectedIcon)
        {
            var controller = CreateController();
            var firstIsolate = DbMock.Sendings.First(s => !s.Deleted).Isolate;
            firstIsolate.ReportStatus = reportStatus;
            var request = new DefaultDataTablesRequest {Search = new Search("", false), Columns = new ColumnCollection(new List<Column>()), Length = 10};

            var result = controller.DataTableAjax(request);

            var firstRow = ((DataTablesResponse)result.Data).data.Cast<dynamic>().First();
            ((string)firstRow.ReportGenerated).Should().Contain(expectedIcon);
        }

        [Test]
        public void LaboratoryExport_EmptyQuery_ForwardToViewWithQueryForLastYear()
        {
            var controller = CreateController();

            var result = controller.LaboratoryExport(new FromToQuery());

            var viewResult= result.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            var query = viewResult.Model.Should().BeOfType<FromToQuery>().And.Subject.As<FromToQuery>();
            query.From.Month.Should().Be(1);
            query.From.Year.Should().Be(DateTime.Now.Year - 1);
            query.To.Month.Should().Be(12);
            query.To.Year.Should().Be(DateTime.Now.Year - 1);
        }

        [Test]
        public void LaboratoryExport_ValidQuery_CreatesExcel()
        {
            var tempExcel = new FileInfo(Path.Combine(TemporaryDirectoryToStoreTestData, "LaboratoryExport.xlsx"));
            var controller = CreateController();

            var result = controller.LaboratoryExport(new FromToQuery {From = DateTime.Now.AddYears(-100), To = DateTime.Now});

            var fileResult = result.Should().BeOfType<FileContentResult>().And.Subject.As<FileContentResult>();
            fileResult.FileContents.Length.Should().BeGreaterThanOrEqualTo(10);
            File.WriteAllBytes(tempExcel.FullName, fileResult.FileContents);

            using (var fastExcel = new FastExcel.FastExcel(tempExcel))
            {
                fastExcel.Worksheets.Length.Should().Be(1);
            }
        }

        [Test]
        public void PubMlstExport_EmptyQuery_ForwardToViewWithQueryForLastYear()
        {
            var controller = CreateController();

            var result = controller.LaboratoryExport(new FromToQuery());

            var viewResult= result.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            var query = viewResult.Model.Should().BeOfType<FromToQuery>().And.Subject.As<FromToQuery>();
            query.From.Month.Should().Be(1);
            query.From.Year.Should().Be(DateTime.Now.Year - 1);
            query.To.Month.Should().Be(12);
            query.To.Year.Should().Be(DateTime.Now.Year - 1);
        }

        [Test]
        public void PubMlstExport_ValidQuery_CreatesExcel()
        {
            var tempExcel = new FileInfo(Path.Combine(TemporaryDirectoryToStoreTestData, "PubMLSTExport.xlsx"));
            var controller = CreateController();

            var result = controller.PubMlstExport(new FromToQueryWithAdjustment {From = DateTime.Now.AddYears(-100), To = DateTime.Now});

            var fileResult = result.Should().BeOfType<FileContentResult>().And.Subject.As<FileContentResult>();
            fileResult.FileContents.Length.Should().BeGreaterThanOrEqualTo(10);
            File.WriteAllBytes(tempExcel.FullName, fileResult.FileContents);

            using (var fastExcel = new FastExcel.FastExcel(tempExcel))
            {
                fastExcel.Worksheets.Length.Should().Be(1);
            }
        }

        [Test]
        public void RkiExport_EmptyQuery_ForwardToViewWithQueryForLastYear()
        {
            var controller = CreateController();

            var result = controller.RkiExport(new FromToQueryWithAdjustment());

            var viewResult = result.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            var query = viewResult.Model.Should().BeOfType<FromToQueryWithAdjustment>().And.Subject.As<FromToQuery>();
            query.From.Month.Should().Be(1);
            query.From.Year.Should().Be(DateTime.Now.Year - 1);
            query.To.Month.Should().Be(12);
            query.To.Year.Should().Be(DateTime.Now.Year - 1);
        }

        [Test]
        public void RkiExport_ValidQuery_CreatesExcel()
        {
            var tempExcel = new FileInfo(Path.Combine(TemporaryDirectoryToStoreTestData, "LaboratoryExport.xlsx"));
            var controller = CreateController();

            var result = controller.RkiExport(new FromToQueryWithAdjustment { From = DateTime.Now.AddYears(-100), To = DateTime.Now });

            var fileResult = result.Should().BeOfType<FileContentResult>().And.Subject.As<FileContentResult>();
            fileResult.FileContents.Length.Should().BeGreaterThanOrEqualTo(10);
            File.WriteAllBytes(tempExcel.FullName, fileResult.FileContents);

            using (var fastExcel = new FastExcel.FastExcel(tempExcel))
            {
                fastExcel.Worksheets.Length.Should().Be(1);
            }
        }

        // specs/002 FR-002
        [Test]
        public void Create_NewPatientSending_PrefillsDefaults()
        {
            var controller = CreateIsolatedController(new IntakeDbStub());

            var model = controller.Create().As<ViewResult>().Model.As<PatientSendingViewModel<Patient, Sending>>();

            model.Sending.ReceivingDate.Date.Should().Be(DateTime.Today);
            model.Sending.SamplingDate.Should().HaveValue();
            model.Sending.SamplingDate.Value.Date.Should().Be(DateTime.Today.AddDays(-7));
            model.Sending.SenderSpecies.Should().Be("H. influenzae");
            model.Patient.HibVaccination.Should().Be(VaccinationStatus.NotStated);
        }

        // specs/002 FR-003: laboratory number of the current year, no stem number
        [Test]
        public void Create_ValidModel_LinksSenderAndPatientAndAssignsLaboratoryNumberButNoStemNumber()
        {
            var db = new IntakeDbStub();
            var controller = CreateIsolatedController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.Sending.SamplingDate = new DateTime(2020, 3, 10);
            intake.Sending.ReceivingDate = new DateTime(2020, 3, 12);

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
            var patient = db.Patients.Should().ContainSingle().Subject;
            var sending = db.Sendings.Should().ContainSingle().Subject;
            sending.SenderId.Should().Be(SenderId);
            sending.PatientId.Should().Be(patient.PatientId);
            sending.Isolate.Should().NotBeNull();
            sending.Isolate.LaboratoryNumber.Should().Be($"001/{DateTime.Now:yy}");
            sending.Isolate.StemNumber.Should().BeNull();
        }

        // specs/002 FR-009, Key Entities (initials stored upper-cased)
        [Test]
        public void Create_LowerCaseInitialsOfOneExistingPatient_PausesForDuplicateResolution()
        {
            var db = new IntakeDbStub();
            db.Patients.Add(CreatePatient(42, "Y.D."));
            var controller = CreateIsolatedController(db);

            var result = controller.Create(CreateIntake(100, "y.d."));

            var model = result.Should().BeOfType<ViewResult>().Subject.Model.As<PatientSendingViewModel<Patient, Sending>>();
            model.DuplicatePatientDetected.Should().BeTrue();
            model.Patient.Initials.Should().Be("Y.D.");
            db.Patients.Should().ContainSingle();
            db.Sendings.Should().BeEmpty();
        }

        // specs/002 FR-010
        [Test]
        public void Create_UseExistingPatient_AttachesSendingToExistingPatientAndUpdatesIt()
        {
            var db = new IntakeDbStub();
            db.Patients.Add(CreatePatient(42, "Y.D."));
            var controller = CreateIsolatedController(db);
            var intake = CreateIntake(100, "Y.D.");
            intake.Patient.City = "SV-002-Ort";
            intake.DuplicatePatientResolution = DuplicatePatientResolution.UseExistingPatient;

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
            db.Patients.Should().ContainSingle();
            db.ModifiedEntities.OfType<Patient>().Should().ContainSingle()
                .Which.Should().Match<Patient>(p => p.PatientId == 42 && p.City == "SV-002-Ort");
            db.Sendings.Should().ContainSingle().Which.PatientId.Should().Be(42);
        }

        // specs/002 FR-010
        [Test]
        public void Create_CreateNewPatient_CreatesSecondPatientWithSameIdentifyingDetails()
        {
            var db = new IntakeDbStub();
            db.Patients.Add(CreatePatient(42, "Y.D."));
            var controller = CreateIsolatedController(db);
            var intake = CreateIntake(100, "Y.D.");
            intake.DuplicatePatientResolution = DuplicatePatientResolution.CreateNewPatient;

            var result = controller.Create(intake);

            result.Should().BeOfType<RedirectToRouteResult>();
            db.Patients.Should().HaveCount(2);
            db.Sendings.Should().ContainSingle().Which.PatientId.Should().NotBe(42);
        }

        // specs/002 FR-011 (without a posted resolution; see BACKLOG DATA-031 for the bypass)
        [Test]
        public void Create_TwoExistingPatientsMatch_RejectsThirdPatient()
        {
            var db = new IntakeDbStub();
            db.Patients.Add(CreatePatient(42, "Y.D."));
            db.Patients.Add(CreatePatient(43, "Y.D."));
            var controller = CreateIsolatedController(db);

            var result = controller.Create(CreateIntake(100, "Y.D."));

            result.Should().BeOfType<ViewResult>();
            controller.ModelState[""].Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().StartWith("Es existieren bereits zwei Patienten");
            db.Patients.Should().HaveCount(2);
            db.Sendings.Should().BeEmpty();
        }

        // specs/002 FR-012
        [Test]
        public void Create_LaboratoryNumberTakenMeanwhile_WarnsWithNewLaboratoryNumber()
        {
            var db = new IntakeDbStub();
            db.Isolates.Add(new Isolate { Year = DateTime.Now.Year, YearlySequentialIsolateNumber = 1 });
            var controller = CreateIsolatedController(db);
            var intake = CreateIntake(100, "X.B.");
            intake.Sending.LaboratoryNumber = $"001/{DateTime.Now:yy}";

            controller.Create(intake);

            controller.TempData["WarningMessage"].Should().Be(
                $"Beim Speichern der letzten Einsendung hat sich die Labornummer geändert. Neue Labornummer: 002/{DateTime.Now:yy}");
        }

        // specs/002 FR-012
        [Test]
        public void Create_LaboratoryNumberAsShown_DoesNotWarn()
        {
            var controller = CreateIsolatedController(new IntakeDbStub());
            var intake = CreateIntake(100, "X.B.");
            intake.Sending.LaboratoryNumber = $"001/{DateTime.Now:yy}";

            controller.Create(intake);

            controller.TempData.ContainsKey("WarningMessage").Should().BeFalse();
        }

        // specs/002 FR-004
        [Test]
        public void Edit_SendingOfDeletedSender_OffersActiveSendersAndTheAttachedOne()
        {
            var db = new IntakeDbStub();
            db.Senders.Add(new Sender { SenderId = 1, Name = "Active" });
            db.Senders.Add(new Sender { SenderId = 2, Name = "Deleted, attached", Deleted = true });
            db.Senders.Add(new Sender { SenderId = 3, Name = "Deleted, unrelated", Deleted = true });
            var sending = CreateStoredSending(100, 2, CreatePatient(42, "X.A."));
            db.Patients.Add(sending.Patient);
            db.Sendings.Add(sending);

            var edit = CreateIsolatedController(db).Edit(100).As<ViewResult>();
            var create = CreateIsolatedController(db).Create().As<ViewResult>();

            ((IEnumerable<Sender>) edit.ViewBag.PossibleSenders).Select(s => s.SenderId).Should().BeEquivalentTo(new[] { 1, 2 });
            ((IEnumerable<Sender>) create.ViewBag.PossibleSenders).Select(s => s.SenderId).Should().BeEquivalentTo(new[] { 1 });
        }

        // specs/002 FR-015
        [Test]
        public void Edit_ValidModel_SavesPatientAndSendingAndRedirectsToList()
        {
            var db = new IntakeDbStub();
            var controller = CreateIsolatedController(db);
            var intake = CreateIntake(100, "X.A.");
            intake.Patient.PatientId = 42;
            intake.Sending.PatientId = 42;

            var result = controller.Edit(intake);

            result.Should().BeOfType<RedirectToRouteResult>().Which.RouteValues["action"].Should().Be("Index");
            db.ModifiedEntities.Should().BeEquivalentTo(new object[] { intake.Patient, intake.Sending });
            db.SavedWithoutValidation.Should().Equal(false);
        }

        // specs/002 FR-016, FR-018
        [Test]
        public void DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation()
        {
            var db = new IntakeDbStub();
            var sending = new Sending { SendingId = 100, SenderLaboratoryNumber = null, SenderSpecies = null, Patient = CreatePatient(42, null) };
            db.Sendings.Add(sending);
            var controller = CreateIsolatedController(db);

            var result = controller.DeleteConfirmed(100);

            result.Should().BeOfType<RedirectToRouteResult>().Which.RouteValues["action"].Should().Be("Index");
            sending.Deleted.Should().BeTrue();
            db.SavedWithoutValidation.Should().Equal(true);
        }

        // specs/002 FR-018, US3.4
        [Test]
        public void Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation()
        {
            var db = new IntakeDbStub();
            var sending = new Sending { SendingId = 100, Deleted = true, SenderLaboratoryNumber = null, SenderSpecies = null, Patient = CreatePatient(42, null) };
            db.Sendings.Add(sending);
            var controller = CreateIsolatedController(db);

            var result = controller.Undelete(100);

            result.Should().BeOfType<RedirectToRouteResult>().Which.RouteValues["action"].Should().Be("Index");
            sending.Deleted.Should().BeFalse();
            db.SavedWithoutValidation.Should().Equal(true);
        }

        // specs/002 FR-013, FR-016
        [Test]
        public void DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns()
        {
            var db = new IntakeDbStub();
            db.Senders.Add(new Sender { SenderId = SenderId, Name = "SV-002", PostalCode = "97080" });
            var active = CreateStoredSending(100, SenderId, CreatePatient(42, "X.A."));
            active.SenderLaboratoryNumber = "SV-002-A1";
            var deleted = CreateStoredSending(101, SenderId, CreatePatient(43, "X.B."));
            deleted.Deleted = true;
            db.Sendings.Add(active);
            db.Sendings.Add(deleted);
            var controller = CreateIsolatedController(db);
            var request = new DefaultDataTablesRequest { Search = new Search("", false), Columns = new ColumnCollection(new List<Column>()), Length = 10 };

            var response = (DataTablesResponse) controller.DataTableAjax(request).Data;

            response.recordsTotal.Should().Be(1);
            var row = response.data.Cast<dynamic>().Single();
            ((string) row.Initials).Should().Be("X.A.");
            ((string) row.PatientPostalCode).Should().Be("97082");
            ((string) row.SenderPostalCode).Should().Be("97080");
            ((string) row.SenderLaboratoryNumber).Should().Be("SV-002-A1");
            ((string) row.Invasive).Should().Be("Ja");
        }

        private const int SenderId = 7;

        private static Patient CreatePatient(int patientId, string initials)
        {
            return new Patient
            {
                PatientId = patientId,
                Initials = initials,
                BirthDate = new DateTime(2003, 3, 3),
                PostalCode = "97082",
                Gender = Gender.Female
            };
        }

        private static PatientSendingViewModel<Patient, Sending> CreateIntake(int sendingId, string initials)
        {
            return new PatientSendingViewModel<Patient, Sending>
            {
                Patient = CreatePatient(0, initials),
                Sending = new Sending
                {
                    SendingId = sendingId,
                    SenderId = SenderId,
                    SenderLaboratoryNumber = "SV-002",
                    SamplingLocation = SamplingLocation.Blood
                }
            };
        }

        private static Sending CreateStoredSending(int sendingId, int senderId, Patient patient)
        {
            return new Sending
            {
                SendingId = sendingId,
                SenderId = senderId,
                Patient = patient,
                PatientId = patient.PatientId,
                SenderLaboratoryNumber = "SV-002",
                Isolate = new Isolate { IsolateId = sendingId, Year = 2026, YearlySequentialIsolateNumber = sendingId }
            };
        }

        private PatientSendingController CreateIsolatedController(IntakeDbStub db)
        {
            var controller = new PatientSendingController(db, new PatientController(db), new SendingController(db));
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

            public IDbSet<Patient> Patients { get; } = new InMemoryDbSet<Patient>(new HashSet<Patient>())
            {
                FindFunction = (patients, keys) => patients.FirstOrDefault(p => p.PatientId == (int) keys[0])
            };

            public IDbSet<Sending> Sendings { get; } = new InMemoryDbSet<Sending>(new HashSet<Sending>())
            {
                FindFunction = (sendings, keys) => sendings.FirstOrDefault(s => s.SendingId == (int) keys[0])
            };

            public IDbSet<Isolate> Isolates { get; } = new InMemoryDbSet<Isolate>(new HashSet<Isolate>());
            public IDbSet<County> Counties { get; } = new InMemoryDbSet<County>(new HashSet<County>());
            public IDbSet<HealthOffice> HealthOffices { get; } = new InMemoryDbSet<HealthOffice>(new HashSet<HealthOffice>());
            public IDbSet<ConfigurationEntry> ConfigurationEntries { get; } = new InMemoryDbSet<ConfigurationEntry>(new HashSet<ConfigurationEntry>());
            public IDbSet<EpsilometerTest> EpsilometerTests { get; } = new InMemoryDbSet<EpsilometerTest>(new HashSet<EpsilometerTest>());
            public IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints { get; } = new InMemoryDbSet<EucastClinicalBreakpoint>(new HashSet<EucastClinicalBreakpoint>());
            public IDbSet<MeningoPatient> MeningoPatients { get; } = new InMemoryDbSet<MeningoPatient>(new HashSet<MeningoPatient>());
            public IDbSet<MeningoSending> MeningoSendings { get; } = new InMemoryDbSet<MeningoSending>(new HashSet<MeningoSending>());
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

        private PatientSendingController CreateController()
        {
            var request = Substitute.For<HttpRequestBase>();
            request.Form.Returns(requestForm);
            var context = Substitute.For<HttpContextBase>();
            var server = Substitute.For<HttpServerUtilityBase>();
            context.Request.Returns(request);
            context.Server.Returns(server);
            var controller = new PatientSendingController(DbMock, patientController, sendingController);
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            var urlHelper = Substitute.For<UrlHelper>();
            urlHelper.Action(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);
            urlHelper.Action(Arg.Any<string>(), Arg.Any<object>()).Returns(string.Empty);

            controller.Url = urlHelper;
            return controller;
        }

        public string TemporaryDirectoryToStoreTestData { get; set; }
    }
}