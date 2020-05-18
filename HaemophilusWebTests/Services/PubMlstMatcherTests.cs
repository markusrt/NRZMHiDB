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
        private ApplicationDbContextMock _database;

        private DateTime _firstDayMorning = new DateTime(2010, 10, 10, 0, 0, 0);
        private PubMlstService _pubMlstService;

        [SetUp]
        public void Setup()
        {
            _database = new ApplicationDbContextMock();
            MockData.CreateMockData(_database);

            for (int i = 0; i < 5; i++)
            {
                var isolate = MockData.CreateInstance<MeningoIsolate>();
                var sending = MockData.CreateInstance<MeningoSending>();
                sending.SamplingDate = i == 3 ? null : (DateTime?)_firstDayMorning.AddDays(i).AddHours(8);
                sending.ReceivingDate = _firstDayMorning.AddDays(i).AddHours(8);
                isolate.Sending = sending;
                isolate.StemNumber = i == 4 ? null : (int?) i+1;
                isolate.NeisseriaPubMlstIsolate = null;
                _database.MeningoIsolates.Add(isolate);
            }

            _pubMlstService = Substitute.For<PubMlstService>();
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            var sut = CreatePubMlstMatcher();

            sut.Should().NotBeNull();
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(5, 5)]
        public void Match_FromToMatchingXDaysRecord_ReturnsListWithXEntries(int days, int count)
        {
            var sut = CreatePubMlstMatcher();

            var result = sut.Match(new FromToQuery { From = _firstDayMorning, To = _firstDayMorning.AddDays(days) });

            result.Should().HaveCount(count);
        }

        [Test]
        public void Match_WithCorrespondingPupMlstRecord_FindsMatchAndCreatesPubMlstIsolate()
        {
            var sut = CreatePubMlstMatcher();
            var pubMlstIsolate = MockData.CreateInstance<NeisseriaPubMlstIsolate>();
            pubMlstIsolate.PubMlstId = 18377;
            _pubMlstService.GetIsolateByReference("DE1").Returns(pubMlstIsolate);

            var result = sut.Match(new FromToQuery { From = _firstDayMorning, To = _firstDayMorning.AddDays(1) });

            var match = result.First();
            match.PubMlstId.Should().Be(18377);
            match.StemNumber.Should().Be("DE1");
            
            var pubMlstIsolateId = match.NeisseriaPubMlstIsolateId.Should().HaveValue().And.Subject;
            _database.NeisseriaPubMlstIsolates.Find(pubMlstIsolateId).Should().NotBeNull();

            var isolate = _database.MeningoIsolates.First();
            isolate.NeisseriaPubMlstIsolate.Should().NotBeNull();
            match.LaboratoryNumber.Should().Be(isolate.LaboratoryNumberWithPrefix);
        }

        [Test]
        public void Match_WithEmptyStemNumber_DoesNotCallPubMlstService()
        {
            var sut = CreatePubMlstMatcher();
            var pubMlstIsolate = MockData.CreateInstance<NeisseriaPubMlstIsolate>();
            pubMlstIsolate.PubMlstId = 18377;
            _pubMlstService.GetIsolateByReference("DE1").Returns(pubMlstIsolate);

            var result = sut.Match(new FromToQuery { From = _firstDayMorning, To = _firstDayMorning.AddDays(5) });

            var match = result.Last();
            match.PubMlstId.Should().NotHaveValue();
            match.NeisseriaPubMlstIsolateId.Should().NotHaveValue();
            match.StemNumber.Should().Be("DE -");
            match.LaboratoryNumber.Should().Be(_database.MeningoIsolates.Last().LaboratoryNumberWithPrefix);

            _pubMlstService.Received(4).GetIsolateByReference(Arg.Any<string>());
        }

        [Test]
        public void Match_WithoutCorrespondingPupMlstRecord_ReturnsMatchWithEmptyPubMlstId()
        {
            var sut = CreatePubMlstMatcher();

            var result = sut.Match(new FromToQuery { From = _firstDayMorning, To = _firstDayMorning.AddDays(1) });

            var match = result.First();

            match.PubMlstId.Should().NotHaveValue();
            match.NeisseriaPubMlstIsolateId.Should().NotHaveValue();
            match.StemNumber.Should().Be("DE1");

            var isolate = _database.MeningoIsolates.First();
            match.IsolateId.Should().Be(isolate.MeningoIsolateId);
            _database.MeningoIsolates.First().NeisseriaPubMlstIsolate.Should().BeNull();
            match.LaboratoryNumber.Should().Be(isolate.LaboratoryNumberWithPrefix);
        }

        private PubMlstMatcher CreatePubMlstMatcher()
        {
            return new PubMlstMatcher(_database, _pubMlstService);
        }
    }
}