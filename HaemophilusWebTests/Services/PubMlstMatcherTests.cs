using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Services
{
    public class PubMlstMatcherTests
    {
        private static readonly DateTime FirstDayMorning = new DateTime(2010, 10, 10, 0, 0, 0);

        [Test]
        public void Ctor_DoesNotThrow()
        {
            var sut = CreatePubMlstMatcher(out _, out _);

            sut.Should().NotBeNull();
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(5, 5)]
        public void Match_FromToMatchingXDaysRecord_ReturnsListWithXEntries(int days, int count)
        {
            var sut = CreatePubMlstMatcher(out _, out _);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(days) });

            result.Should().HaveCount(count);
        }

        [Test]
        public void Match_WithCorrespondingPupMlstRecord_FindsMatchAndCreatesPubMlstIsolate()
        {
            var sut = CreatePubMlstMatcher(out var pubMlstService, out var database);

            var pubMlstIsolate = MockData.CreateInstance<NeisseriaPubMlstIsolate>();
            pubMlstIsolate.PubMlstId = 18377;
            pubMlstService.GetIsolateByReference("DE1").Returns(pubMlstIsolate);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(1) });

            var match = result.First();
            match.PubMlstId.Should().Be(18377);
            match.StemNumber.Should().Be("DE1");
            
            var pubMlstIsolateId = match.NeisseriaPubMlstIsolateId.Should().HaveValue().And.Subject;
            database.NeisseriaPubMlstIsolates.Find(pubMlstIsolateId).Should().NotBeNull();

            var isolate = database.MeningoIsolates.First();
            isolate.NeisseriaPubMlstIsolate.Should().NotBeNull();
            match.LaboratoryNumber.Should().Be(isolate.LaboratoryNumberWithPrefix);
        }

        [Test]
        public void Match_WithEmptyStemNumber_DoesNotCallPubMlstService()
        {
            var sut = CreatePubMlstMatcher(out var pubMlstService, out var database);
            var pubMlstIsolate = MockData.CreateInstance<NeisseriaPubMlstIsolate>();
            pubMlstIsolate.PubMlstId = 18377;
            pubMlstService.GetIsolateByReference("DE1").Returns(pubMlstIsolate);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(5) });

            var match = result.Last();
            match.PubMlstId.Should().NotHaveValue();
            match.NeisseriaPubMlstIsolateId.Should().NotHaveValue();
            match.StemNumber.Should().Be("DE -");
            match.LaboratoryNumber.Should().Be(database.MeningoIsolates.Last().LaboratoryNumberWithPrefix);

            pubMlstService.Received(4).GetIsolateByReference(Arg.Any<string>());
        }

        [Test]
        public void Match_WithoutCorrespondingPupMlstRecord_ReturnsMatchWithEmptyPubMlstId()
        {
            var sut = CreatePubMlstMatcher(out _, out var database);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(1) });

            var match = result.First();

            match.PubMlstId.Should().NotHaveValue();
            match.NeisseriaPubMlstIsolateId.Should().NotHaveValue();
            match.StemNumber.Should().Be("DE1");

            var isolate = database.MeningoIsolates.First();
            match.IsolateId.Should().Be(isolate.MeningoIsolateId);
            database.MeningoIsolates.First().NeisseriaPubMlstIsolate.Should().BeNull();
            match.LaboratoryNumber.Should().Be(isolate.LaboratoryNumberWithPrefix);
        }

        [Test]
        public void Match_WithoutPrimaryOrSecondaryPupMlstRecord_ReturnsMatchWithEmptyPubMlstId()
        {
            var sut = CreatePubMlstMatcherWithTwoServices(out var primaryService, out var secondaryService, out var database);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(1) });

            var match = result.First();

            match.PubMlstId.Should().NotHaveValue();
            match.NeisseriaPubMlstIsolateId.Should().NotHaveValue();
            match.StemNumber.Should().Be("DE1");

            var isolate = database.MeningoIsolates.First();
            match.IsolateId.Should().Be(isolate.MeningoIsolateId);
            database.MeningoIsolates.First().NeisseriaPubMlstIsolate.Should().BeNull();
            match.LaboratoryNumber.Should().Be(isolate.LaboratoryNumberWithPrefix);
            primaryService.Received().GetIsolateByReference(isolate.StemNumberWithPrefix);
            secondaryService.Received().GetIsolateByReference(isolate.StemNumberWithPrefix);
        }

        [Test]
        public void Match_WithResultOnPrimaryService_DoesNotCallSecondary()
        {
            var sut = CreatePubMlstMatcherWithTwoServices(out var primaryService, out var secondaryService, out var database);

            var pubMlstIsolate = MockData.CreateInstance<NeisseriaPubMlstIsolate>();
            pubMlstIsolate.PubMlstId = 18377;
            primaryService.GetIsolateByReference("DE1").Returns(pubMlstIsolate);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(1) });

            var match = result.First();
            match.PubMlstId.Should().Be(18377);

            var isolate = database.MeningoIsolates.First();
            secondaryService.DidNotReceive().GetIsolateByReference(Arg.Any<string>());
        }


        
        [Test]
        public void Match_OnSecondaryService_FindsMatchAndCreatesPubMlstIsolate()
        {
            var sut = CreatePubMlstMatcherWithTwoServices(out var primaryService, out var secondaryService, out var database);

            var pubMlstIsolate = MockData.CreateInstance<NeisseriaPubMlstIsolate>();
            pubMlstIsolate.PubMlstId = 18377;
            primaryService.GetIsolateByReference("DE1").Returns((NeisseriaPubMlstIsolate)null);
            secondaryService.GetIsolateByReference("DE1").Returns(pubMlstIsolate);

            var result = sut.Match(new FromToQuery { From = FirstDayMorning, To = FirstDayMorning.AddDays(1) });

            var match = result.First();
            match.PubMlstId.Should().Be(18377);
            match.StemNumber.Should().Be("DE1");
            
            var pubMlstIsolateId = match.NeisseriaPubMlstIsolateId.Should().HaveValue().And.Subject;
            database.NeisseriaPubMlstIsolates.Find(pubMlstIsolateId).Should().NotBeNull();

            var isolate = database.MeningoIsolates.First();
            isolate.NeisseriaPubMlstIsolate.Should().NotBeNull();
            match.LaboratoryNumber.Should().Be(isolate.LaboratoryNumberWithPrefix);
        }

        // specs/019 FR-004, US1.4, Edge Cases "Periods and bounds": sampling date decides, receiving date only without
        // sampling date, both bounds inclusive for dates stored without time of day
        [Test]
        public void Match_SelectsBySamplingDateOrWithoutItByReceivingDate_BoundsInclusive()
        {
            var from = new DateTime(2010, 10, 10);
            var to = new DateTime(2010, 10, 12);
            var onStartDay = CreateIsolate(1, null, from, from.AddDays(2));
            var onEndDay = CreateIsolate(2, null, to, to.AddDays(2));
            var afterEndDay = CreateIsolate(3, null, to.AddDays(1), to.AddDays(1));
            var receivedWithoutSampling = CreateIsolate(4, null, null, to);
            var sampledBeforeReceivedWithin = CreateIsolate(5, null, from.AddDays(-1), from.AddDays(1));
            var receivedBeforeWithoutSampling = CreateIsolate(6, null, null, from.AddDays(-1));
            var db = CreateOwnDatabase(out _, onStartDay, onEndDay, afterEndDay, receivedWithoutSampling,
                sampledBeforeReceivedWithin, receivedBeforeWithoutSampling);
            var sut = new PubMlstMatcher(db, Substitute.For<NeisseriaPubMlstService>());

            var result = sut.Match(new FromToQuery { From = from, To = to });

            result.Select(r => r.IsolateId).Should().BeEquivalentTo(new[] { 1, 2, 4 });
        }

        // specs/019 Edge Cases "Periods and bounds": a start date after the end date selects nothing
        [Test]
        public void Match_StartDateAfterEndDate_SelectsNothing()
        {
            var day = new DateTime(2010, 10, 10);
            var db = CreateOwnDatabase(out _, CreateIsolate(1, 1, day, day));
            var service = Substitute.For<NeisseriaPubMlstService>();
            var sut = new PubMlstMatcher(db, service);

            var result = sut.Match(new FromToQuery { From = day.AddDays(1), To = day.AddDays(-1) });

            result.Should().BeEmpty();
            service.DidNotReceive().GetIsolateByReference(Arg.Any<string>());
        }

        // specs/019 FR-008, US3.2, US3.3, SC-004: a known PubMLST id reuses the local record and overwrites its seventeen
        // typing values, including values that are now empty; id, database name and record count stay
        [Test]
        public void Match_KnownPubMlstId_OverwritesTypingValuesOfExistingRecord()
        {
            var day = new DateTime(2010, 10, 10);
            var existing = CreatePubMlstRecord(18377, "pubmlst_neisseria_isolates", "old");
            existing.NeisseriaPubMlstIsolateId = 5;
            var matched = CreateIsolate(1, 1, day, day);
            matched.NeisseriaPubMlstIsolate = existing;
            var otherLinked = CreateIsolate(2, null, day.AddYears(-1), day.AddYears(-1));
            otherLinked.NeisseriaPubMlstIsolate = existing;
            var db = CreateOwnDatabase(out var records, matched, otherLinked);
            records.Add(existing);
            var remote = CreatePubMlstRecord(18377, "pubmlst_neisseria_isolates", "new");
            remote.NadA = "";
            remote.SequenceType = null;
            remote.TrumenbaReactivity = null;
            var service = Substitute.For<NeisseriaPubMlstService>();
            service.GetIsolateByReference("DE1").Returns(remote);
            var sut = new PubMlstMatcher(db, service);

            var result = sut.Match(new FromToQuery { From = day, To = day });

            records.Should().ContainSingle().Which.Should().BeSameAs(existing);
            existing.NeisseriaPubMlstIsolateId.Should().Be(5);
            existing.PubMlstId.Should().Be(18377);
            existing.Database.Should().Be("pubmlst_neisseria_isolates");
            existing.Should().BeEquivalentTo(remote, o => o.Excluding(r => r.NeisseriaPubMlstIsolateId));
            matched.NeisseriaPubMlstIsolate.Should().BeSameAs(existing);
            otherLinked.NeisseriaPubMlstIsolate.PorAVr1.Should().Be("new-PorAVr1");
            result.Single().NeisseriaPubMlstIsolateId.Should().Be(5);
        }

        // specs/019 FR-009, US1.8, Edge Cases "Previous matches are never removed": an isolate not found keeps its link
        [Test]
        public void Match_IsolateNotFound_KeepsExistingLinkAndValues()
        {
            var day = new DateTime(2010, 10, 10);
            var existing = CreatePubMlstRecord(18377, "pubmlst_neisseria_isolates", "old");
            var isolate = CreateIsolate(1, 1, day, day);
            isolate.NeisseriaPubMlstIsolate = existing;
            var db = CreateOwnDatabase(out var records, isolate);
            records.Add(existing);
            var sut = new PubMlstMatcher(db, Substitute.For<NeisseriaPubMlstService>());

            var result = sut.Match(new FromToQuery { From = day, To = day });

            result.Single().PubMlstId.Should().NotHaveValue();
            isolate.NeisseriaPubMlstIsolate.Should().BeSameAs(existing);
            existing.PorAVr1.Should().Be("old-PorAVr1");
            db.DidNotReceive().SaveChanges();
        }

        // specs/019 FR-009, US1.6: each found isolate is linked and saved before the next isolate is searched
        [Test]
        public void Match_SavesEachFoundIsolateBeforeSearchingTheNext()
        {
            var day = new DateTime(2010, 10, 10);
            var first = CreateIsolate(1, 1, day, day);
            var second = CreateIsolate(2, 2, day, day);
            var db = CreateOwnDatabase(out _, first, second);
            var log = new List<string>();
            db.When(d => d.SaveChanges()).Do(_ => log.Add("save"));
            var service = Substitute.For<NeisseriaPubMlstService>();
            service.GetIsolateByReference(Arg.Any<string>()).Returns(ci =>
            {
                var reference = ci.Arg<string>();
                log.Add("search " + reference);
                return CreatePubMlstRecord(reference == "DE1" ? 101 : 102, "pubmlst_neisseria_isolates", reference);
            });
            var sut = new PubMlstMatcher(db, service);

            sut.Match(new FromToQuery { From = day, To = day });

            log.Should().HaveCount(4);
            log[0].Should().StartWith("search ");
            log[1].Should().Be("save");
            log[2].Should().StartWith("search ");
            log[3].Should().Be("save");
            first.NeisseriaPubMlstIsolate.PubMlstId.Should().Be(101);
            second.NeisseriaPubMlstIsolate.PubMlstId.Should().Be(102);
        }

        // specs/019 FR-012, US3.4: within one run each database is asked at most once per isolate
        [Test]
        public void Match_IsolateNotFound_AsksEachDatabaseOnce()
        {
            var day = new DateTime(2010, 10, 10);
            var db = CreateOwnDatabase(out _, CreateIsolate(1, 1, day, day));
            var primary = Substitute.For<NeisseriaPubMlstService>();
            var secondary = Substitute.For<NeisseriaPubMlstService>();
            var sut = new PubMlstMatcher(db, primary, secondary);

            sut.Match(new FromToQuery { From = day, To = day });

            primary.Received(1).GetIsolateByReference("DE1");
            secondary.Received(1).GetIsolateByReference("DE1");
        }

        // specs/019 FR-012, US3.5, SC-004: a later run searches an already matched isolate again and creates no second record
        [Test]
        public void Match_SecondRun_SearchesMatchedIsolateAgainWithoutSecondRecord()
        {
            var day = new DateTime(2010, 10, 10);
            var isolate = CreateIsolate(1, 1, day, day);
            var db = CreateOwnDatabase(out var records, isolate);
            var service = Substitute.For<NeisseriaPubMlstService>();
            service.GetIsolateByReference("DE1").Returns(
                _ => CreatePubMlstRecord(18377, "pubmlst_neisseria_isolates", "same"));
            var sut = new PubMlstMatcher(db, service);

            sut.Match(new FromToQuery { From = day, To = day });
            sut.Match(new FromToQuery { From = day, To = day });

            service.Received(2).GetIsolateByReference("DE1");
            records.Should().ContainSingle().Which.PorAVr1.Should().Be("same-PorAVr1");
            isolate.NeisseriaPubMlstIsolate.Should().BeSameAs(records.Single());
        }

        private static IApplicationDbContext CreateOwnDatabase(out HashSet<NeisseriaPubMlstIsolate> records,
            params MeningoIsolate[] isolates)
        {
            // Own sets instead of the shared static store of ApplicationDbContextMock
            var db = Substitute.For<IApplicationDbContext>();
            records = new HashSet<NeisseriaPubMlstIsolate>();
            db.MeningoIsolates.Returns(new InMemoryDbSet<MeningoIsolate>(new HashSet<MeningoIsolate>(isolates)));
            db.NeisseriaPubMlstIsolates.Returns(new InMemoryDbSet<NeisseriaPubMlstIsolate>(records));
            return db;
        }

        private static MeningoIsolate CreateIsolate(int id, int? stemNumber, DateTime? samplingDate, DateTime receivingDate)
        {
            return new MeningoIsolate
            {
                MeningoIsolateId = id,
                StemNumber = stemNumber,
                YearlySequentialIsolateNumber = id,
                Year = 2010,
                Sending = new MeningoSending { SamplingDate = samplingDate, ReceivingDate = receivingDate }
            };
        }

        private static NeisseriaPubMlstIsolate CreatePubMlstRecord(int pubMlstId, string database, string prefix)
        {
            return new NeisseriaPubMlstIsolate
            {
                PubMlstId = pubMlstId,
                Database = database,
                PorAVr1 = prefix + "-PorAVr1",
                PorAVr2 = prefix + "-PorAVr2",
                FetAVr = prefix + "-FetAVr",
                PorB = prefix + "-PorB",
                Fhbp = prefix + "-Fhbp",
                Nhba = prefix + "-Nhba",
                NadA = prefix + "-NadA",
                PenA = prefix + "-PenA",
                GyrA = prefix + "-GyrA",
                ParC = prefix + "-ParC",
                ParE = prefix + "-ParE",
                RpoB = prefix + "-RpoB",
                RplF = prefix + "-RplF",
                SequenceType = prefix + "-ST",
                ClonalComplex = prefix + "-cc",
                BexseroReactivity = prefix + "-Bexsero",
                TrumenbaReactivity = prefix + "-Trumenba"
            };
        }

        private static PubMlstMatcher CreatePubMlstMatcher(out PubMlstService pubMlstService, out ApplicationDbContextMock database)
        {
            database = CreateMockDatabase();
            pubMlstService = Substitute.For<NeisseriaPubMlstService>();
            return new PubMlstMatcher(database, pubMlstService);
        }
        
        private static PubMlstMatcher CreatePubMlstMatcherWithTwoServices(out PubMlstService primaryService, out PubMlstService secondaryService, out ApplicationDbContextMock database)
        {
            database = CreateMockDatabase();
            primaryService = Substitute.For<NeisseriaPubMlstService>();
            secondaryService = Substitute.For<NeisseriaPubMlstService>();
            return new PubMlstMatcher(database, primaryService, secondaryService);
        }


        private static ApplicationDbContextMock CreateMockDatabase()
        {
            var database = new ApplicationDbContextMock();
            MockData.CreateMockData(database);

            for (int i = 0; i < 5; i++)
            {
                var isolate = MockData.CreateInstance<MeningoIsolate>();
                var sending = MockData.CreateInstance<MeningoSending>();
                sending.SamplingDate = i == 3 ? null : FirstDayMorning.AddDays(i).AddHours(8);
                sending.ReceivingDate = FirstDayMorning.AddDays(i).AddHours(8);
                isolate.Sending = sending;
                isolate.StemNumber = i == 4 ? null : (int?)i + 1;
                isolate.NeisseriaPubMlstIsolate = null;
                database.MeningoIsolates.Add(isolate);
            }

            return database;
        }
    }
}