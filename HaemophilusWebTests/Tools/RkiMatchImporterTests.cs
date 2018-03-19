using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class RkiMatchImporterTests : ITempDirectoryTest
    {
        private const string TestData =
            "InterneRef;Aktenzeichen;StatusName;klhi_nr;LaufendeNummer;Jahr\n" +
            "311798;100171005575;;;;\n" +
            "11357917;HIN20160107-123268423-Schu;definitiv;2515;;\n" +
            "11357917;HIN20160107-123268423-Schu;wahrscheinlich;2515;;\n" +
            "10376685;S51-021804901;möglich;2466;;\n" +
            "103766775;S51-021804912;möglich;-12;10;2017\n" +
            "103766885;S51-021804901;kein Match;2488;;\n" +
            "10340481;101579019012;wahrscheinlich;2494;;\n" +
            "10340484;101579019012;wahrscheinlich;-234;;\n" +
            "1034048;101579019012;wahrscheinlich;;;\n" +
            ";101579019013;möglich;2494;;";

        public string TemporaryDirectoryToStoreTestData { get; set; }

        private readonly ApplicationDbContextMock _applicationDbContext = new ApplicationDbContextMock();

        public string TestFile => Path.Combine(TemporaryDirectoryToStoreTestData, "import.asc");

        [SetUp]
        public void SetUp()
        {
            _applicationDbContext.SendingDbSet.Clear();
        }

        [Test]
        public void Ctor_DoesNotThrowException()
        {
            CreateImporter();
        }

        private RkiMatchImporter CreateImporter()
        {
            File.WriteAllText(TestFile, TestData, Encoding.UTF8);
            return new RkiMatchImporter(TestFile, _applicationDbContext);
        }

        [TestCase(2515, RkiStatus.Definite, "HIN20160107-123268423-Schu", 11357917, Description = "There are two entries for this case but definite should surpass probable")]
        [TestCase(2466, RkiStatus.Possible, "S51-021804901", 10376685)]
        [TestCase(2488, RkiStatus.None, "S51-021804901", 103766885)]
        [TestCase(2494, RkiStatus.Probable, "101579019012", 10340481)]
        [TestCase(2494, RkiStatus.Probable, "101579019012", 10340481)]
        public void ImportMatches_SendingIsFound_MatchesCorrespondingEntry(int stemNumber, RkiStatus rkiStatus,
            string rkiReferenceNumber, int rkiReferenceId)
        {
            var sending = CreateSendingWithStemNumber(stemNumber);
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiStatus.Should().Be(rkiStatus);
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be(rkiReferenceNumber);
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(rkiReferenceId);
        }

        [Test]
        public void ImportMatches_ExistingRkiMatch_DatabaseSendingIdIsLeftIntact()
        {
            var sending = CreateSendingWithStemNumber(2466);
            sending.RkiMatchRecord = new RkiMatchRecord
            {
                SendingId = 2466,
                RkiStatus = RkiStatus.Possible,
                RkiReferenceId = 123,
                RkiReferenceNumber = "abc"
            };
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();
             
            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.SendingId.Should().Be(2466);
            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.Possible);
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be("S51-021804901");
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(10376685);
        }

        [Test]
        public void ImportMatches_MatchViaYearlySequentialIsolateNumber_MatchesCorrespondingEntry()
        {
            var sending = CreateSendingWithStemNumber(2477);
            sending.Isolate.YearlySequentialIsolateNumber = 10;
            sending.Isolate.Year = 2017;
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();
            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.Possible);
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be("S51-021804912");
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(103766775);
        }

        [TestCase("2008-12-31")]
        [TestCase("2005-01-13")]
        public void ImportMatches_SendingIsBefore2009_DoesNotMatch(string dateString)
        {
            var sending = CreateSendingWithStemNumber(2515);
            sending.ReceivingDate = DateTime.Parse(dateString);

            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().BeNull();
        }

        [Test]
        public void ImportMatches_MultipleSendingsAreFound_MatchesAllEntries()
        {
            _applicationDbContext.Sendings.Add(CreateSendingWithStemNumber(2515));
            _applicationDbContext.Sendings.Add(CreateSendingWithStemNumber(2466));
            _applicationDbContext.Sendings.Add(CreateSendingWithStemNumber(2494));
            var importer = CreateImporter();

            importer.ImportMatches();

            _applicationDbContext.Sendings.Count(s => s.RkiMatchRecord != null).Should().Be(3);
        }

        private static Sending CreateSendingWithStemNumber(int stemNumber)
        {
            var sending = new Sending
            {
                SendingId = stemNumber * 2,
                Isolate = new Isolate {IsolateId = stemNumber * 3, StemNumber = stemNumber}
            };
            return sending;
        }
    }
}