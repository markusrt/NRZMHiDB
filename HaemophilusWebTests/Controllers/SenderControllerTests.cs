using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.Tools;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class SenderControllerTests
    {
        private const int IdOfSenderWithoutSendings = 42;
        private readonly SenderController senderController = new SenderController(DbMock);

        private static readonly ApplicationDbContextMock DbMock = new ApplicationDbContextMock();

        static SenderControllerTests()
        {
            MockData.CreateMockData(DbMock);
            DbMock.Senders.Add(new Sender {SenderId = IdOfSenderWithoutSendings});
        }

        [Test]
        public void Delete_AddsExistingSendingsToViewBag()
        {
            var senderToDelete = DbMock.Sendings.First(s=> !s.Deleted).SenderId;
            var result = senderController.Delete(senderToDelete) as ViewResult;

            var sendingsInViewBag = (List<Sending>)result.ViewBag.Sendings;
            var sendingsOfSender = DbMock.Sendings.Where(s => s.SenderId == senderToDelete).ToList();
            sendingsInViewBag.Count.Should().Be(1);
            sendingsOfSender.Should().BeEquivalentTo(sendingsInViewBag);
        }

        [Test]
        public void Delete_IdOfSenderWithoutSendings_SendingsInViewBagAreEmpty()
        {
            var result = senderController.Delete(IdOfSenderWithoutSendings) as ViewResult;

            ((List<Sending>)result.ViewBag.Sendings).Should().BeEmpty();
        }

        // specs/001 FR-009: active submissions of both databases, deleted ones are not listed
        [Test]
        public void Delete_SenderWithSendingsInBothDatabases_ListsOnlyNotDeletedSendingsOfBoth()
        {
            var db = CreateIsolatedDb(new Sender {SenderId = 1});
            db.Sendings.Add(new Sending {SendingId = 1, SenderId = 1});
            db.Sendings.Add(new Sending {SendingId = 2, SenderId = 1, Deleted = true});
            db.Sendings.Add(new Sending {SendingId = 3, SenderId = 2});
            db.MeningoSendings.Add(new MeningoSending {MeningoSendingId = 1, SenderId = 1});
            db.MeningoSendings.Add(new MeningoSending {MeningoSendingId = 2, SenderId = 1, Deleted = true});

            var result = (ViewResult)new SenderController(db).Delete(1);

            ((List<Sending>)result.ViewBag.Sendings).Select(s => s.SendingId).Should().Equal(1);
            ((List<MeningoSending>)result.ViewBag.MeningoSendings).Select(s => s.MeningoSendingId).Should().Equal(1);
        }

        // specs/001 FR-018: missing id is a bad request
        [Test]
        public void SingleSenderActions_WithoutId_ReturnBadRequest()
        {
            var controller = new SenderController(CreateIsolatedDb());

            new[] {controller.Details(null), controller.Edit((int?)null), controller.Delete(null), controller.Undelete(null)}
                .Should().AllSatisfy(r => ((HttpStatusCodeResult)r).StatusCode.Should().Be(400));
        }

        // specs/001 FR-018: unknown id is not found
        [Test]
        public void SingleSenderActions_WithUnknownId_ReturnNotFound()
        {
            var controller = new SenderController(CreateIsolatedDb(new Sender {SenderId = 1}));

            new[] {controller.Details(99), controller.Edit((int?)99), controller.Delete(99), controller.Undelete(99)}
                .Should().AllBeOfType<HttpNotFoundResult>();
        }

        // specs/001 FR-002, FR-008, FR-011: active list without deleted senders, deleted list with them only
        [Test]
        public void IndexAndDeleted_SplitSendersByDeletedFlag()
        {
            var controller = new SenderController(CreateIsolatedDb(
                new Sender {SenderId = 1}, new Sender {SenderId = 2, Deleted = true}));

            ((IEnumerable<Sender>)((ViewResult)controller.Index()).Model).Select(s => s.SenderId).Should().Equal(1);
            ((IEnumerable<Sender>)((ViewResult)controller.Deleted()).Model).Select(s => s.SenderId).Should().Equal(2);
        }

        // specs/001 FR-008, FR-012: soft delete, saved with save validation switched off
        [Test]
        public void DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation()
        {
            var sender = new Sender {SenderId = 1};
            var db = CreateIsolatedDb(sender);

            var result = new SenderController(db).DeleteConfirmed(1);

            sender.Deleted.Should().BeTrue();
            db.SavedWithoutValidation.Should().Equal(true);
            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Index");
        }

        // specs/001 FR-011, FR-012: restore clears the flag, saved with save validation switched off
        [Test]
        public void Undelete_InvalidDeletedSender_ClearsDeletedFlagAndSavesWithoutValidation()
        {
            var sender = new Sender {SenderId = 1, Deleted = true};
            var db = CreateIsolatedDb(sender);

            var result = new SenderController(db).Undelete(1);

            sender.Deleted.Should().BeFalse();
            db.SavedWithoutValidation.Should().Equal(true);
            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Index");
        }

        // specs/020 FR-006, Edge Cases "Restore of an active record": restoring an active sender is not refused and
        // saves it unchanged
        [Test]
        public void Undelete_ActiveSender_StaysActiveAndRedirectsToList()
        {
            var sender = new Sender {SenderId = 1, Name = "SV-020"};
            var db = CreateIsolatedDb(sender);

            var result = new SenderController(db).Undelete(1);

            sender.Deleted.Should().BeFalse();
            sender.Name.Should().Be("SV-020");
            db.SavedWithoutValidation.Should().Equal(true);
            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Index");
        }

        // specs/020 US4.5: the inherited Meningococci restore clears the deleted mark of the shared sender
        [Test]
        public void MeningoUndelete_DeletedSender_ClearsDeletedFlagOfSharedSender()
        {
            var sender = new Sender {SenderId = 1, Deleted = true};
            var db = CreateIsolatedDb(sender);

            new MeningoSenderController(db).Undelete(1);

            sender.Deleted.Should().BeFalse();
            db.SavedWithoutValidation.Should().Equal(true);
        }

        // specs/001 FR-017: create and edit bind only the editable fields, never the deleted flag
        [TestCase("Create")]
        [TestCase("Edit")]
        public void PostActions_BindEditableFieldsOnly(string actionName)
        {
            var postAction = typeof(SenderController).GetMethods()
                .Single(m => m.Name == actionName && m.GetCustomAttributes(typeof(HttpPostAttribute), true).Any());
            var bind = (BindAttribute)postAction.GetParameters().Single()
                .GetCustomAttributes(typeof(BindAttribute), false).Single();

            bind.Include.Split(',').Should().BeEquivalentTo("SenderId", "Name", "Department", "StreetWithNumber",
                "PostalCode", "City", "Phone1", "Phone2", "Fax", "Email", "Remark");
        }

        // specs/001 FR-014: export without a range proposes the whole previous calendar year
        [Test]
        public void Export_WithoutRange_DefaultsToPreviousCalendarYear()
        {
            var lastYear = DateTime.Now.Year - 1;

            var result = (ViewResult)new SenderController(CreateIsolatedDb()).Export(new FromToQuery());

            var query = (FromToQuery)result.Model;
            query.From.Should().Be(new DateTime(lastYear, 1, 1));
            query.To.Should().Be(new DateTime(lastYear, 12, 31));
        }

        private static readonly string[] DocumentedSenderExportColumns =
        {
            "Einsendernummer", "Name", "Abteilung", "Straße", "Postleitzahl", "Ort", "Telefon #1", "Telefon #2",
            "Fax", "E-Mail", "Bemerkung", "Anzahl Einsendungen", "Stammnummern", "Labornummern"
        };

        private static readonly FromToQuery Range2022 =
            new FromToQuery {From = new DateTime(2022, 1, 1), To = new DateTime(2022, 12, 31)};

        // specs/013 FR-006, FR-007, FR-010, US1.2, US1.6, SC-004: file name per pathogen and the 14 columns in order
        [TestCase(false, "Einsender-Haemophilus-Export_20220101-20221231.xlsx")]
        [TestCase(true, "Einsender-Meningokokken-Export_20220101-20221231.xlsx")]
        public void Export_WithRange_WritesDocumentedFileNameAndColumns(bool meningococci, string expectedFileName)
        {
            var db = CreateIsolatedDb(new Sender {SenderId = 1, Name = "SV-013"});
            var controller = meningococci
                ? (System.Web.Mvc.ControllerBase)new MeningoSenderController(db)
                : new SenderController(db);

            var result = meningococci
                ? ((MeningoSenderController)controller).Export(Range2022)
                : ((SenderController)controller).Export(Range2022);
            var table = SenderExportTable(controller, Range2022);

            ((FileContentResult)result).FileDownloadName.Should().Be(expectedFileName);
            table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Should().Equal(DocumentedSenderExportColumns);
        }

        // specs/013 FR-003, FR-005, US1.3, US1.4, US2.2, SC-002: active senders with an active submission of the
        // pathogen whose sampling date (or, without one, receiving date) lies in the range, both ends inclusive
        [Test]
        public void Export_SelectsActiveSendersBySamplingOrReceivingDateOfThePathogen()
        {
            var db = CreateIsolatedDb(
                new Sender {SenderId = 1}, new Sender {SenderId = 2}, new Sender {SenderId = 3},
                new Sender {SenderId = 4, Deleted = true}, new Sender {SenderId = 5}, new Sender {SenderId = 6});
            AddSending(db, 1, 1, sampled: new DateTime(2022, 1, 1), received: new DateTime(2022, 1, 3));
            AddSending(db, 2, 1, sampled: new DateTime(2022, 12, 31), received: new DateTime(2023, 1, 2));
            AddSending(db, 3, 2, sampled: null, received: new DateTime(2022, 12, 31));
            AddSending(db, 4, 3, sampled: new DateTime(2021, 12, 31), received: new DateTime(2022, 1, 2));
            AddSending(db, 5, 3, sampled: null, received: new DateTime(2023, 1, 1));
            AddSending(db, 6, 4, sampled: new DateTime(2022, 3, 10), received: new DateTime(2022, 3, 12));
            db.MeningoSendings.Add(new MeningoSending
            {
                MeningoSendingId = 1, SenderId = 5, SamplingDate = new DateTime(2022, 5, 10),
                ReceivingDate = new DateTime(2022, 5, 12), Isolate = new MeningoIsolate()
            });

            var table = SenderExportTable(new SenderController(db), Range2022);

            table.Rows.Cast<DataRow>().Select(r => r["Einsendernummer"]).Should().Equal(1, 2);
        }

        // specs/013 FR-006, FR-007, US1.7, US2.1, US2.3: current contact data, then count, stem numbers without prefix
        // ("-" when absent) and prefixed laboratory numbers of the export's pathogen only, in the same order
        [Test]
        public void Export_RowListsContactDataAndNumbersOfThePathogenOnly()
        {
            var db = CreateIsolatedDb(new Sender
            {
                SenderId = 7, Name = "SV-013-Alpha", Department = "SV-013 Abteilung", StreetWithNumber = "Teststraße 13",
                PostalCode = "97080", City = "Würzburg", Phone1 = "0931 1300", Phone2 = "0931 1301",
                Fax = "0931 1302", Email = "sv013@example.org", Remark = "SV-013-neu"
            });
            AddSending(db, 1, 7, new DateTime(2022, 1, 1), new DateTime(2022, 1, 3), stemNumber: 31301, sequentialNumber: 24);
            AddSending(db, 2, 7, null, new DateTime(2022, 12, 31), stemNumber: null, sequentialNumber: 25);
            db.MeningoSendings.Add(new MeningoSending
            {
                MeningoSendingId = 1, SenderId = 7, SamplingDate = new DateTime(2022, 5, 10),
                ReceivingDate = new DateTime(2022, 5, 12),
                Isolate = new MeningoIsolate {StemNumber = 31301, YearlySequentialIsolateNumber = 20, Year = 2026}
            });

            var haemophilusRow = SenderExportTable(new SenderController(db), Range2022).Rows.Cast<DataRow>().Single();
            var meningoRow = SenderExportTable(new MeningoSenderController(db), Range2022).Rows.Cast<DataRow>().Single();

            haemophilusRow.ItemArray.Should().Equal(7, "SV-013-Alpha", "SV-013 Abteilung", "Teststraße 13", "97080",
                "Würzburg", "0931 1300", "0931 1301", "0931 1302", "sv013@example.org", "SV-013-neu", 2L,
                "31301,-", "KL024/26,KL025/26");
            meningoRow.ItemArray.Skip(11).Should().Equal(1L, "31301", "MZ020/26");
        }

        private static void AddSending(SenderDbStub db, int sendingId, int senderId, DateTime? sampled,
            DateTime received, int? stemNumber = null, int sequentialNumber = 1)
        {
            db.Sendings.Add(new Sending
            {
                SendingId = sendingId, SenderId = senderId, SamplingDate = sampled, ReceivingDate = received,
                Isolate = new Isolate {StemNumber = stemNumber, YearlySequentialIsolateNumber = sequentialNumber, Year = 2026}
            });
        }

        /// <summary>
        ///     Runs the export's sender selection and column definition (private members of the shared base
        ///     controller) and returns the table that the spreadsheet writer receives.
        /// </summary>
        private static DataTable SenderExportTable(System.Web.Mvc.ControllerBase controller, FromToQuery query)
        {
            var baseType = controller.GetType().BaseType;
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var senders = (List<Sender>)baseType.GetMethod("SendersMatchingExportQuery", flags)
                .Invoke(controller, new object[] {query});
            var definition = (ExportDefinition<Sender>)baseType.GetMethod("CreateExportDefinition", flags)
                .Invoke(controller, new object[] {query});
            return definition.ToDataTable(senders);
        }

        private static SenderDbStub CreateIsolatedDb(params Sender[] senders)
        {
            var db = new SenderDbStub();
            foreach (var sender in senders)
            {
                db.Senders.Add(sender);
            }
            return db;
        }

        /// <summary>
        ///     Context with its own (non-static) sets, so these tests do not share data with
        ///     <see cref="ApplicationDbContextMock" />; records whether SaveChanges ran without save validation.
        /// </summary>
        private class SenderDbStub : IApplicationDbContext
        {
            private bool validationDisabled;

            public readonly List<bool> SavedWithoutValidation = new List<bool>();

            public IDbSet<Sender> Senders { get; } = new InMemoryDbSet<Sender>(new HashSet<Sender>())
            {
                FindFunction = (senders, keys) => senders.FirstOrDefault(s => s.SenderId == (int) keys[0])
            };

            public IDbSet<Patient> Patients { get; } = new InMemoryDbSet<Patient>(new HashSet<Patient>());
            public IDbSet<Sending> Sendings { get; } = new InMemoryDbSet<Sending>(new HashSet<Sending>());
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