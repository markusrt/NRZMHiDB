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
            "InterneRef;Aktenzeichen;StatusName;klhi_nr\n" +
            "311798;100171005575;;\n" +
            "11357917;HIN20160107-123268423-Schu;definitiv;2515\n" +
            "10376685;S51-021804901;möglich;2466\n" +
            "10340481;101579019012;wahrscheinlich;2494\n" +
            "10340484;101579019012;wahrscheinlich;-234";

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

        [TestCase(2515, RkiStatus.Definite, "HIN20160107-123268423-Schu", 11357917)]
        [TestCase(2466, RkiStatus.Possible, "S51-021804901", 10376685)]
        [TestCase(2494, RkiStatus.Probable, "101579019012", 10340481)]
        public void ImportMatches_SendingIsFound_MatchesCorrespondingEntry(int sendingId, RkiStatus rkiStatus, string rkiReferenceNumber, int rkiReferenceId)
        {
            var sending = new Sending {SendingId = sendingId};
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().NotBeNull();
            sending.RkiMatchRecord.RkiStatus.Should().Be(rkiStatus);
            sending.RkiMatchRecord.RkiReferenceNumber.Should().Be(rkiReferenceNumber);
            sending.RkiMatchRecord.RkiReferenceId.Should().Be(rkiReferenceId);
        }

        [TestCase("2008-12-31")]
        [TestCase("2005-01-13")]
        [TestCase(null)]
        public void ImportMatches_SendingIsBefore2009_DoesNotMatch(string dateString)
        {
            var sending = new Sending
            {
                SendingId = 2515,
                SamplingDate = dateString==null ? (DateTime?) null : DateTime.Parse(dateString)
            };
            _applicationDbContext.Sendings.Add(sending);
            var importer = CreateImporter();

            importer.ImportMatches();

            sending.RkiMatchRecord.Should().BeNull();
        }

        [Test]
        public void ImportMatches_MultipleSendingsAreFound_MatchesAllEntries()
        {
            _applicationDbContext.Sendings.Add(new Sending { SendingId = 2515 });
            _applicationDbContext.Sendings.Add(new Sending { SendingId = 2466 });
            _applicationDbContext.Sendings.Add(new Sending { SendingId = 2494 });
            var importer = CreateImporter();

            importer.ImportMatches();

            _applicationDbContext.Sendings.Count(s => s.RkiMatchRecord != null).Should().Be(3);

        }

    }
}