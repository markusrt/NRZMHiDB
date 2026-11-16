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
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    public class ReportControllerTests : ITempDirectoryTest
    {
        private const int IsolateId = 10;
        private const string Report1 = "report1.docx";
        private const string Report2 = "report2.docx";

        private static ApplicationDbContextMock DbMock;

        private static readonly Catalog Catalog = new Catalog();

        private ReportController controller;

        public string TemporaryDirectoryToStoreTestData { get; set; }

        private static void CreateMockData()
        {
            var isolate = DbMock.Isolates.SingleOrDefault(i => i.IsolateId == IsolateId)
                ?? Catalog.CreateInstance<Isolate>();
            isolate.ReportDate = null;
            isolate.ReportStatus = ReportStatus.None;
            isolate.IsolateId = IsolateId;

            DbMock.Isolates.Add(isolate);

            var senderId = isolate.Sending.SenderId;
            var sender = DbMock.Senders.SingleOrDefault(s => s.SenderId == senderId)
                ?? Catalog.CreateInstance<Sender>();
            sender.SenderId = senderId;
            DbMock.Senders.Add(sender);
        }

        private void CreateMockReportTemplates()
        {
            CreateMockReportTemplate(Report1);
            CreateMockReportTemplate(Report2);
        }

        private void CreateMockReportTemplate(string name)
        {
            using (var file = File.Create(Path.Combine(TemporaryDirectoryToStoreTestData, name), 1, FileOptions.RandomAccess))
            {
                file.Close();
            }
        }

        [OneTimeSetUp]
        public void InitializeAutomapper()
        {
            MvcApplication.Mapper.Should().NotBeNull();
        }

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            IsolateViewModelMappingActionBase.DbForTest = DbMock;
            controller = new ReportController(DbMock);
            var context = Substitute.For<HttpContextBase>();
            var server = Substitute.For<HttpServerUtilityBase>();
            server.MapPath(ReportController.ReportTemplatesPath).Returns(TemporaryDirectoryToStoreTestData);
            context.Server.Returns(server);
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            CreateMockReportTemplates();

            CreateMockData();
        }

        [Test]
        public void Isolate_ReportTemplatesAvailable_AddsFileInfosToViewBag()
        {
            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var reportTemplates = viewResult.ViewBag.ReportTemplates as List<FileInfo>;
            reportTemplates.Count.Should().Be(2);
            reportTemplates.Should().Contain(f => f.FullName.EndsWith(Report1));
            reportTemplates.Should().Contain(f => f.FullName.EndsWith(Report2));
        }

        [Test]
        public void Isolate_ValidModel_FillsInterpretationProperties()
        {
            var isolate = DbMock.Isolates.SingleOrDefault(i => i.IsolateId == IsolateId);
            isolate.Agglutination = SerotypeAgg.Negative;
            isolate.BexA = TestResult.NotDetermined;
            isolate.SerotypePcr = SerotypePcr.NotDetermined;
            isolate.Agglutination = SerotypeAgg.Negative;
            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var model = viewResult.Model as IsolateViewModel;
            model.InterpretationPreliminary.Should().Contain("Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).");
            model.InterpretationPreliminary.Should().NotContain("Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
            model.Interpretation.Should().Contain("Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
        }

        [Test]
        public void Isolate_NoReportTemplatesAvailable_AddsFileInfosToViewBag()
        {
            Directory.Delete(TemporaryDirectoryToStoreTestData, true);
            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var reportTemplates = viewResult.ViewBag.ReportTemplates as List<FileInfo>;
            reportTemplates.Count.Should().Be(0);
        }

        // specs/010 FR-002, US1.4 (the list is read on every request, not cached)
        [Test]
        public void Isolate_TemplateAddedBetweenRequests_IsListedOnNextRequest()
        {
            controller.Isolate(IsolateId);
            CreateMockReportTemplate("report3.docx");

            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var reportTemplates = viewResult.ViewBag.ReportTemplates as List<FileInfo>;
            reportTemplates.Select(f => f.Name).Should().BeEquivalentTo(Report1, Report2, "report3.docx");
        }

        // specs/010 FR-004
        [Test]
        public void Isolate_WithoutId_StillDiscoversTemplates()
        {
            var result = controller.Isolate(null) as HttpStatusCodeResult;

            result.StatusCode.Should().Be(400);
            var reportTemplates = controller.ViewBag.ReportTemplates as List<FileInfo>;
            reportTemplates.Should().HaveCount(2);
        }

        [Test]
        public void ReportGenerated_ValidIsolateId_AssignsReportDate()
        {
            controller.ReportGenerated(IsolateId);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
            isolate.ReportStatus.Should().Be(ReportStatus.Final);
        }

        [Test]
        public void ReportGenerated_MultipleReports_UpdatesReportDate()
        {
            controller.ReportGenerated(IsolateId);
            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            var firstReportDate = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
            isolate.ReportDate = firstReportDate;

            controller.ReportGenerated(IsolateId);

            isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().BeAfter(firstReportDate);
        }

        [Test]
        public void ReportGenerated_ValidIsolateIdAndPreliminaryReport_DoesNotAssignReportDate()
        {
            controller.ReportGenerated(IsolateId, true);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.HasValue.Should().Be(false);
            isolate.ReportStatus.Should().Be(ReportStatus.Preliminary);
        }

        [Test]
        public void ReportGenerated_PreliminaryReportAfterFinal_DoesNotResetStateNorReportDate()
        {
            controller.ReportGenerated(IsolateId);
            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            var originalReportDate = isolate.ReportDate;

            controller.ReportGenerated(IsolateId, true);

            isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportDate.Should().Be(originalReportDate);
            isolate.ReportStatus.Should().Be(ReportStatus.Final);
        }


        [TestCase(1)]
        [TestCase(null)]
        [TestCase(-1)]
        public void ReportGenerated_InvalidIsolateId_DoesNotThrow(int? isolateId)
        {
            controller.Invoking(c => c.ReportGenerated(isolateId)).Should().NotThrow();
        }

        // specs/008 FR-003
        [Test]
        public void Isolate_WithoutId_ReturnsBadRequest()
        {
            var result = controller.Isolate(null) as HttpStatusCodeResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        // specs/008 FR-003
        [Test]
        public void Isolate_UnknownId_ReturnsNotFound()
        {
            var result = controller.Isolate(4711);

            result.Should().BeOfType<HttpNotFoundResult>();
        }

        // specs/008 FR-005
        [Test]
        public void Isolate_ConfiguredReportSigners_AddsSignersToViewBag()
        {
            DbMock.ConfigurationEntries.Add(new ConfigurationEntry
            {
                Key = HaemophilusWeb.Services.ConfigurationService.Keys.ReportSigners,
                Value = "[\"Signer One\",\"Signer Two\"]"
            });

            var viewResult = controller.Isolate(IsolateId) as ViewResult;

            var signers = viewResult.ViewBag.ReportSigners as List<string>;
            signers.Should().Equal("Signer One", "Signer Two");
        }

        // specs/008 FR-019, Report State Transitions (unknown/missing identifier)
        [TestCase(4711, false)]
        [TestCase(4711, true)]
        [TestCase(null, false)]
        [TestCase(null, true)]
        public void ReportGenerated_UnknownOrMissingIsolateId_AcknowledgesAndChangesNoIsolate(int? isolateId, bool preliminary)
        {
            var result = controller.ReportGenerated(isolateId, preliminary);

            result.Data.Should().Be(true);
            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportStatus.Should().Be(ReportStatus.None);
            isolate.ReportDate.Should().BeNull();
        }

        // specs/008 FR-016, Report State Transitions (Preliminary -> Final)
        [Test]
        public void ReportGenerated_FinalAfterPreliminary_SetsFinalAndReportDate()
        {
            controller.ReportGenerated(IsolateId, true);

            controller.ReportGenerated(IsolateId);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportStatus.Should().Be(ReportStatus.Final);
            isolate.ReportDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        // specs/008 FR-017, Report State Transitions (Preliminary -> Preliminary)
        [Test]
        public void ReportGenerated_PreliminaryAfterPreliminary_KeepsPreliminaryWithoutReportDate()
        {
            controller.ReportGenerated(IsolateId, true);

            controller.ReportGenerated(IsolateId, true);

            var isolate = DbMock.Isolates.Single(i => i.IsolateId == IsolateId);
            isolate.ReportStatus.Should().Be(ReportStatus.Preliminary);
            isolate.ReportDate.Should().BeNull();
        }

        // specs/016: the health-office lookup runs with the real RkiTool and a stubbed RKI query function, so the
        // client's parsing is exercised; the health-office set is private to each test.
        private const string ForchheimAddress = "Landratsamt Forchheim\nGesundheitsamt\nAm Streckerplatz 3\n91301 Forchheim";

        private class HealthOfficeLookup
        {
            public ReportController Controller;
            public IApplicationDbContext Db;
            public InMemoryDbSet<HealthOffice> Offices;
            public readonly List<string> QueriedPostalCodes = new List<string>();
        }

        private static string RkiPage(string name)
        {
            using (var stream = typeof(ReportControllerTests).Assembly.GetManifestResourceStream($"HaemophilusWeb.Resources.{name}.html"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static HealthOfficeLookup CreateHealthOfficeLookup(Func<string, string> rkiAnswer, params HealthOffice[] stored)
        {
            var lookup = new HealthOfficeLookup
            {
                Offices = new InMemoryDbSet<HealthOffice>(new HashSet<HealthOffice>(stored)),
                Db = Substitute.For<IApplicationDbContext>()
            };
            lookup.Db.HealthOffices.Returns(lookup.Offices);
            lookup.Db.When(d => d.PerformWithoutSaveValidation(Arg.Any<Action>())).Do(c => c.Arg<Action>()());
            lookup.Controller = new ReportController(lookup.Db);
            typeof(ReportControllerBase<Isolate, IsolateViewModel>)
                .GetField("rkiTool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(lookup.Controller, new HaemophilusWeb.Tools.RkiTool(postalCode =>
                {
                    lookup.QueriedPostalCodes.Add(postalCode);
                    return rkiAnswer(postalCode);
                }));
            return lookup;
        }

        // specs/016 FR-006, US1.5, Lookup Outcome (no stored record with the same address)
        [Test]
        public void QueryHealthOffice_NoStoredRecordWithSameAddress_StoresWebsiteValuesWithoutValidationAndReturnsThem()
        {
            var lookup = CreateHealthOfficeLookup(_ => RkiPage("RkiToolQuerySingleResult"));

            var result = lookup.Controller.QueryHealthOffice("91301");

            var stored = lookup.Offices.Should().ContainSingle().Subject;
            stored.Address.Should().Be(ForchheimAddress);
            stored.Phone.Should().Be("09191 86-3504");
            stored.Fax.Should().Be("09191 86-3508");
            stored.Email.Should().Be("Gesundheitsamt@lra-fo.de");
            stored.PostalCode.Should().Be("91301");
            result.Data.Should().BeSameAs(stored);
            lookup.Db.Received(1).PerformWithoutSaveValidation(Arg.Any<Action>());
            lookup.Db.Received(1).SaveChanges();
        }

        // specs/016 FR-005, US1.2, US1.4, US2.7: the website is always asked first; a stored record whose address
        // equals the website address with line breaks ignored (also CR LF as the edit form stores it) is returned
        // with its corrected values, and nothing is stored.
        [Test]
        public void QueryHealthOffice_StoredRecordWithSameAddress_ReturnsStoredRecordAndStoresNothing()
        {
            var corrected = new HealthOffice
            {
                HealthOfficeId = 7,
                Address = "Landratsamt Forchheim\r\nGesundheitsamt\r\nAm Streckerplatz 3\r\n91301 Forchheim",
                Phone = "09191 86-9999",
                Fax = "09191 86-9998",
                Email = "korrigiert@example.org",
                PostalCode = "91077"
            };
            var lookup = CreateHealthOfficeLookup(_ => RkiPage("RkiToolQuerySingleResult"), corrected);

            var result = lookup.Controller.QueryHealthOffice("91301");

            result.Data.Should().BeSameAs(corrected);
            lookup.QueriedPostalCodes.Should().Equal("91301");
            lookup.Offices.Should().ContainSingle();
            lookup.Db.DidNotReceive().SaveChanges();
        }

        // specs/016 SC-002, US2.7 (the stored postal code of the first lookup, UX-005, is deliberately not asserted)
        [Test]
        public void QueryHealthOffice_TwoPostalCodesOfSameOffice_StoreOneRecordAndShowCorrectionForBoth()
        {
            var lookup = CreateHealthOfficeLookup(_ => RkiPage("RkiToolQuerySingleResult"));

            lookup.Controller.QueryHealthOffice("91301");
            lookup.Offices.Single().Phone = "09191 86-1111";
            var second = lookup.Controller.QueryHealthOffice("91077");

            var stored = lookup.Offices.Should().ContainSingle().Subject;
            second.Data.Should().BeSameAs(stored);
            ((HealthOffice) second.Data).Phone.Should().Be("09191 86-1111");
            lookup.QueriedPostalCodes.Should().Equal("91301", "91077");
        }

        // specs/016 FR-013, US2.8: a record whose address was changed no longer matches; a new record is stored
        [Test]
        public void QueryHealthOffice_StoredAddressChanged_StoresAndReturnsNewRecordFromWebsite()
        {
            var changed = new HealthOffice
            {
                Address = "Landratsamt Forchheim\nGesundheitsamt\nAm Streckerplatz 5\n91301 Forchheim",
                Phone = "09191 86-9999",
                PostalCode = "91301"
            };
            var lookup = CreateHealthOfficeLookup(_ => RkiPage("RkiToolQuerySingleResult"), changed);

            var result = lookup.Controller.QueryHealthOffice("91301");

            lookup.Offices.Should().HaveCount(2);
            var returned = (HealthOffice) result.Data;
            returned.Should().NotBeSameAs(changed);
            returned.Address.Should().Be(ForchheimAddress);
            returned.Phone.Should().Be("09191 86-3504");
            changed.Phone.Should().Be("09191 86-9999");
        }

        // specs/016 US3.5: after deleting a record, the next lookup stores a fresh record from the website values
        [Test]
        public void QueryHealthOffice_StoredRecordDeleted_StoresFreshRecordWithWebsiteValues()
        {
            var lookup = CreateHealthOfficeLookup(_ => RkiPage("RkiToolQuerySingleResult"));
            lookup.Controller.QueryHealthOffice("91301");
            var first = lookup.Offices.Single();
            first.Phone = "09191 86-1111";
            lookup.Offices.Remove(first);

            var result = lookup.Controller.QueryHealthOffice("91301");

            var fresh = lookup.Offices.Should().ContainSingle().Subject;
            fresh.Should().NotBeSameAs(first);
            fresh.Phone.Should().Be("09191 86-3504");
            result.Data.Should().BeSameAs(fresh);
        }

        // specs/016 FR-002, US1.7, SC-003: no office, failing query or unreadable page -> no office, nothing stored
        [TestCase("no office")]
        [TestCase("query fails")]
        [TestCase("unreadable page")]
        public void QueryHealthOffice_WebsiteGivesNoUsableOffice_ReturnsNullAndStoresNothing(string situation)
        {
            Func<string, string> answer;
            switch (situation)
            {
                case "no office":
                    answer = _ => RkiPage("RkiToolQueryNoResult");
                    break;
                case "query fails":
                    answer = _ => throw new System.Net.WebException("unreachable");
                    break;
                default:
                    answer = _ => "<html><body><div id='result'><select><option>Amt</option></select></div></body></html>";
                    break;
            }
            var lookup = CreateHealthOfficeLookup(answer);

            var result = lookup.Controller.QueryHealthOffice("91301");

            result.Data.Should().BeNull();
            lookup.Offices.Should().BeEmpty();
            lookup.Db.DidNotReceive().SaveChanges();
        }
    }
}