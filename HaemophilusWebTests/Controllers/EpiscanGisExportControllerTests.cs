using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FluentAssertions;
using Geolocation;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using HaemophilusWeb.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using TestDataGenerator;

namespace HaemophilusWeb.Controllers
{
    public class EpiscanGisExportControllerTests : ITempDirectoryTest
    {
        private ApplicationDbContextMock _db = new();

        private IGeonamesService _geonamesService;
        
        public string TemporaryDirectoryToStoreTestData { get; set; }

        [SetUp]
        public void SetUp()
        {
            _geonamesService = Substitute.For<IGeonamesService>();
            _geonamesService.QueryCoordinateByPostalCode(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new Coordinate(49.71754, 11.05877));
            _db = new ApplicationDbContextMock();
            MockData.CreateMockData(_db);
            foreach (var sending in _db.MeningoSendings)
            {
                sending.Isolate = MockData.CreateInstance<MeningoIsolate>();
                sending.Isolate.EpsilometerTests.Clear();
                sending.Isolate.EpsilometerTests.Add(MockData.CreateInstance<EpsilometerTest>());
                sending.Isolate.Sending = sending;

                sending.SamplingDate = new DateTime(2019, 05, 02);
                sending.SamplingLocation = MeningoSamplingLocation.Blood;
                sending.Deleted = false;

                SetupToInterpretAsSerogroup(sending, MeningoSerogroupPcr.C);
            }
        }


        [Test]
        public void Ctor_DoesNotThrow()
        {
            var sut = CreateController();

            sut.Should().NotBeNull();
        }


        [Test]
        public void Download_ReturnsFileContent()
        {
            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            var sut = CreateController();
            
            var actionResult = sut.Download();

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            File.ReadAllLines(testFile).Should().HaveCount(_db.MeningoSendings.Count());
        }

        [Test]
        public void Download_FormatsCsvCorrectly()
        {
            var firstEntry = _db.MeningoSendings.First();
            firstEntry.SamplingLocation = MeningoSamplingLocation.Blood;
            firstEntry.SamplingDate = new DateTime(2022, 1, 2);
            firstEntry.ReceivingDate = new DateTime(2022, 01, 10);
            firstEntry.Patient.BirthDate = new DateTime(2012, 1, 1);
            SetupToInterpretAsSerogroup(firstEntry, MeningoSerogroupPcr.C);

            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            var sut = CreateController();
            
            var content = sut.Download().Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);

            var firstLine = File.ReadAllLines(testFile).First();
            firstLine.Should().Be("1;10.01.2022;02.01.2022;C;22;14;5-5;10;49.71754;11.05877");
        }

        [Test]
        public void DownloadWithOneNoneInvasiveSamplingLocation_ReturnsOneLineLess()
        {
            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            var sut = CreateController();
            _db.MeningoSendings.First().SamplingLocation = MeningoSamplingLocation.EarSwab;
            
            var actionResult = sut.Download();

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            File.ReadAllLines(testFile).Should().HaveCount(_db.MeningoSendings.Count()-1);
        }
        private static void SetupToInterpretAsSerogroup(MeningoSending sending, MeningoSerogroupPcr serogroupPcr)
        {
            var isolate = sending.Isolate;
            isolate.GrowthOnBloodAgar = Growth.No;
            isolate.GrowthOnMartinLewisAgar = Growth.No;
            isolate.Oxidase = TestResult.NotDetermined;
            isolate.Agglutination = MeningoSerogroupAgg.NotDetermined;
            isolate.Onpg = TestResult.NotDetermined;
            isolate.GammaGt = TestResult.NotDetermined;
            isolate.SerogroupPcr = serogroupPcr;
            isolate.MaldiTof = UnspecificTestResult.NotDetermined;
            isolate.PorAPcr = NativeMaterialTestResult.Negative;
            isolate.FetAPcr = NativeMaterialTestResult.Positive;
            isolate.PorAVr1 = "22";
            isolate.PorAVr2 = "14";
            isolate.FetAVr = "5-5";
        }

        private EpiscanGisExportController CreateController()
        {
            return new EpiscanGisExportController(_db, _geonamesService)
            { 
                Url = Substitute.For<UrlHelper>()
            };
        }

    }
}