using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
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