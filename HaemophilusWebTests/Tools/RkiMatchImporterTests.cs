using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using NSubstitute;
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

        private const string Header = "InterneRef;Aktenzeichen;StatusName;klhi_nr;LaufendeNummer;Jahr";

        private RkiMatchImporter CreateImporter(string content, Encoding encoding)
        {
            File.WriteAllText(TestFile, content, encoding);
            return new RkiMatchImporter(TestFile, _applicationDbContext);
        }

        // specs/024 FR-004, US2.6: an unknown status word in a row that is applied ends the import with the helper's message
        [Test]
        public void ImportMatches_UnknownStatusInAppliedRow_ThrowsWithStatusMessage()
        {
            _applicationDbContext.Sendings.Add(CreateSendingWithStemNumber(2515));
            var importer = CreateImporter(Header + "\n11357917;AZ-1;xyz;2515;;", Encoding.UTF8);

            Action action = () => importer.ImportMatches();

            action.Should().Throw<Exception>().WithMessage(
                "'xyz' is not a valid value for 'RkiStatus'; valid values are 'None (0), Possible (1), Probable (2), Definite (3)'");
        }

        // specs/024 FR-004, US2.6: the status word is interpreted only for rows that are applied; an unknown word in a
        // row that matches no submission does not stop the import
        [Test]
        public void ImportMatches_UnknownStatusInRowWithoutSubmission_IsIgnored()
        {
            var sending = CreateSendingWithStemNumber(2515);
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter(Header + "\n11357917;AZ-1;definitiv;2515;;\n99;AZ-2;xyz;-99;;", Encoding.UTF8);

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.Definite);
        }

        // specs/024 FR-004, US2.5: an empty status cell in a row that is applied is stored as "kein Match"
        // (the empty-status row of the test table carries no stem number and is skipped)
        [Test]
        public void ImportMatches_EmptyStatusInAppliedRow_StoresNone()
        {
            var sending = CreateSendingWithStemNumber(2515);
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter(Header + "\n11357917;AZ-1;;2515;;", Encoding.UTF8);

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.None);
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(11357917);
        }

        // specs/024 FR-003, US2.4: a numeric cell that is not a whole number ends the import
        [Test]
        public void ImportMatches_NonNumericStemNumber_ThrowsFormatException()
        {
            _applicationDbContext.Sendings.Add(CreateSendingWithStemNumber(2515));
            var importer = CreateImporter(Header + "\n11357917;AZ-1;definitiv;2515;;\n99;AZ-2;definitiv;12x;;", Encoding.UTF8);

            Action action = () => importer.ImportMatches();

            action.Should().Throw<FormatException>();
        }

        // specs/024 FR-002, Edge Cases "Header spelled differently": header names are recognized regardless of case
        [Test]
        public void ImportMatches_HeaderInUpperCase_IsRecognized()
        {
            var sending = CreateSendingWithStemNumber(2515);
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter(
                "INTERNEREF;AKTENZEICHEN;STATUSNAME;KLHI_NR;LAUFENDENUMMER;JAHR\n11357917;AZ-1;wahrscheinlich;2515;;",
                Encoding.UTF8);

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(11357917);
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be("AZ-1");
            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.Probable);
        }

        // specs/024 FR-002, US2.1: a file without byte order mark and with CR LF line ends is read like the test table
        // (the other tests write the file with a UTF-8 byte order mark and LF line ends)
        [Test]
        public void ImportMatches_FileWithoutByteOrderMarkAndWithCrLf_IsRead()
        {
            var sending = CreateSendingWithStemNumber(2477);
            sending.Isolate.YearlySequentialIsolateNumber = 10;
            sending.Isolate.Year = 2017;
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter(Header + "\r\n103766775;AZ-ö;möglich;-12;10;2017\r\n", new UTF8Encoding(false));

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be("AZ-ö");
            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.Possible);
        }

        // specs/024 FR-008, SC-003: a stored record is not changed by a row of lower rank from a later upload
        [Test]
        public void ImportMatches_ExistingDefiniteRecordAndLowerRow_LeavesRecordUnchanged()
        {
            var sending = CreateSendingWithStemNumber(2488);
            sending.RkiMatchRecord = new RkiMatchRecord
            {
                RkiStatus = RkiStatus.Definite,
                RkiReferenceId = 123,
                RkiReferenceNumber = "abc"
            };
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();

            sending.RkiMatchRecord.RkiStatus.Should().Be(RkiStatus.Definite);
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(123);
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be("abc");
        }

        // specs/024 US3.6: a lookup that finds more than one submission ends the import
        [Test]
        public void ImportMatches_StemNumberFoundTwice_ThrowsInvalidOperationException()
        {
            _applicationDbContext.Sendings.Add(CreateSendingWithStemNumber(2515));
            var duplicate = CreateSendingWithStemNumber(2515);
            duplicate.SendingId = 99991;
            duplicate.Isolate.IsolateId = 99992;
            _applicationDbContext.Sendings.Add(duplicate);
            var importer = CreateImporter();

            Action action = () => importer.ImportMatches();

            action.Should().Throw<InvalidOperationException>();
        }

        // specs/024 FR-009: the whole import runs inside one transaction of the database context
        [Test]
        public void ImportMatches_RunsInsideOneTransaction()
        {
            var context = Substitute.For<IApplicationDbContext>();
            File.WriteAllText(TestFile, TestData, Encoding.UTF8);
            var importer = new RkiMatchImporter(TestFile, context);

            importer.ImportMatches();

            context.Received(1).WrapInTransaction(Arg.Any<Action>());
            context.DidNotReceive().SaveChanges();
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