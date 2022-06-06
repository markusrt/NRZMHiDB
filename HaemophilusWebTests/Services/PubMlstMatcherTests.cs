using System;
using System.Linq;
using FluentAssertions;
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