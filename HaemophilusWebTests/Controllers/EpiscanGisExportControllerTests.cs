using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Geolocation;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.TestUtils;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class EpiscanGisExportControllerTests : ITempDirectoryTest
    {
        private const string ValidToken = "Token";
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

                sending.SamplingDate = new DateTime(2019, 10, 02);
                sending.SamplingLocation = MeningoSamplingLocation.Blood;
                sending.Material = MeningoMaterial.VitalStem;
                sending.Deleted = false;

                SetupToInterpretAsSerogroup(sending, MeningoSerogroupPcr.C);
            }

            ConfigurationManager.AppSettings["RegistrationToken"] = ValidToken;
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
            
            var actionResult = sut.Index(ValidToken);

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            File.ReadAllLines(testFile).Should().HaveCount(_db.MeningoSendings.Count());
        }

        
        [Test]
        public void DownloadWithInvalidToken_Returns404()
        {
            var sut = CreateController();
            
            var actionResult = sut.Index("Invalid" + ValidToken);

            var content = actionResult.Should().BeOfType<HttpNotFoundResult>();
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
            
            var content = sut.Index(ValidToken).Should().BeOfType<FileContentResult>().Subject.FileContents;
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
            
            var actionResult = sut.Index(ValidToken);

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            File.ReadAllLines(testFile).Should().HaveCount(_db.MeningoSendings.Count()-1);
        }

        [Test]
        public void DownloadWithTwoEntriesMissingSerogroup_ReturnsTwoLineLess()
        {
            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            var sut = CreateController();
            _db.MeningoSendings.First().Isolate.SerogroupPcr = MeningoSerogroupPcr.NotDetermined;
            _db.MeningoSendings.Skip(1).First().Isolate.SerogroupPcr = MeningoSerogroupPcr.NotDetermined;
            
            var actionResult = sut.Index(ValidToken);

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            File.ReadAllLines(testFile).Should().HaveCount(_db.MeningoSendings.Count()-2);
        }


        
        [Test]
        public void DownloadWithTwoOfSamePatientMissingSerogroup_AddsPatientOnlyOnceAndChoosesTheOneWithMostData()
        {
            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            var sut = CreateController();
            
            var firstEntry = _db.MeningoSendings.First();
            SetupToInterpretAsSerogroup(firstEntry, MeningoSerogroupPcr.A);
            firstEntry.Isolate.PorAVr1 = string.Empty;

            var secondEntry = _db.MeningoSendings.Skip(1).First();
            SetupToInterpretAsSerogroup(secondEntry, MeningoSerogroupPcr.B);
            secondEntry.MeningoPatientId = firstEntry.MeningoPatientId;
            
            var actionResult = sut.Index(ValidToken);

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            var lineOfSamePatient = File.ReadAllLines(testFile).Where(l => l.StartsWith($"{firstEntry.MeningoPatientId};"));
            lineOfSamePatient.Should().HaveCount(1);

            var exportedLineForThisPatient = lineOfSamePatient.Single();
            exportedLineForThisPatient.Should().Match("*;B;22;14;5-5;*");
        }

        // specs/007 FR-014: submissions whose normalized serogroup is "NG" or "cnl" are skipped
        [Test]
        public void DownloadWithEntriesInterpretedAsNgAndCnl_ReturnsTwoLinesLess()
        {
            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            var sut = CreateController();
            SetupToInterpretAsSerogroup(_db.MeningoSendings.First(), MeningoSerogroupPcr.Negative);
            SetupToInterpretAsSerogroup(_db.MeningoSendings.Skip(1).First(), MeningoSerogroupPcr.Cnl);

            var actionResult = sut.Index(ValidToken);

            var content = actionResult.Should().BeOfType<FileContentResult>().Subject.FileContents;
            File.WriteAllBytes(testFile, content);
            var lines = File.ReadAllLines(testFile);
            lines.Should().HaveCount(_db.MeningoSendings.Count() - 2);
            lines.Should().NotContain(l => l.Contains(";NG;") || l.Contains(";cnl;"));
        }

        // specs/023 FR-001, US1.2: a missing, empty or differently spelled token is answered with "not found"
        [TestCase(null)]
        [TestCase("")]
        [TestCase("token")]
        [TestCase("TOKEN")]
        [TestCase("Token ")]
        public void DownloadWithMissingOrDifferentToken_Returns404(string token)
        {
            var sut = CreateController();

            var actionResult = sut.Index(token);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        // specs/023 FR-002, US1.3-US1.5: plain retrieval only, anonymous, no role check, no anti-forgery check
        [Test]
        public void Index_AdmitsAnonymousGetWithoutRoleOrAntiForgeryCheck()
        {
            var method = typeof(EpiscanGisExportController).GetMethod(nameof(EpiscanGisExportController.Index));

            method.GetCustomAttributes(typeof(HttpGetAttribute), true).Should().HaveCount(1);
            method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Should().HaveCount(1);
            method.GetCustomAttributes(typeof(HttpPostAttribute), true).Should().BeEmpty();
            method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().BeEmpty();
            method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true).Should().BeEmpty();
            typeof(EpiscanGisExportController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().BeEmpty();
        }

        // specs/023 FR-003, US2.2: soft-deleted submissions are left out
        [Test]
        public void DownloadWithOneSoftDeletedSending_ReturnsOneLineLess()
        {
            var sut = CreateController();
            _db.MeningoSendings.First().Deleted = true;

            var lines = ExportLines(sut);

            lines.Should().HaveCount(_db.MeningoSendings.Count() - 1);
            lines.Should().NotContain(l => l.StartsWith($"{_db.MeningoSendings.First().MeningoPatientId};"));
        }

        // specs/023 FR-003, US2.3: "Blut" and "Liquor" are exported
        [TestCase(MeningoSamplingLocation.Blood)]
        [TestCase(MeningoSamplingLocation.Liquor)]
        public void DownloadWithBloodOrLiquor_ExportsEverySending(MeningoSamplingLocation samplingLocation)
        {
            foreach (var sending in _db.MeningoSendings)
            {
                sending.SamplingLocation = samplingLocation;
            }
            var sut = CreateController();

            var lines = ExportLines(sut);

            lines.Should().HaveCount(_db.MeningoSendings.Count());
        }

        // specs/023 FR-004, US2.4, Edge Case "Receiving date earlier than 2019-10-01 with a later sampling date":
        // the sampling date decides when it exists, otherwise the receiving date; no end date.
        // The exact boundary day 2019-10-01 is left out on purpose (asymmetric boundary, DEC-008).
        [TestCase("2019-10-15", "2019-10-20", true)]
        [TestCase("2019-09-15", "2019-09-20", false)]
        [TestCase(null, "2019-10-15", true)]
        [TestCase(null, "2019-09-15", false)]
        [TestCase("2019-10-15", "2019-09-15", true)]
        [TestCase("2019-09-15", "2019-10-15", false)]
        [TestCase("2030-01-01", "2030-01-05", true)]
        public void Download_SelectsByTimeWindow(string samplingDate, string receivingDate, bool expectedInFile)
        {
            var sending = _db.MeningoSendings.First();
            sending.SamplingDate = samplingDate == null ? null : DateTime.Parse(samplingDate, CultureInfo.InvariantCulture);
            sending.ReceivingDate = DateTime.Parse(receivingDate, CultureInfo.InvariantCulture);
            var sut = CreateController();

            var lines = ExportLines(sut);

            lines.Any(l => l.StartsWith($"{sending.MeningoPatientId};")).Should().Be(expectedInFile);
            lines.Should().HaveCount(_db.MeningoSendings.Count() - (expectedInFile ? 0 : 1));
        }

        // specs/023 FR-006, US2.6, US5.3, SC-005: a case without coordinate is left out, the run completes
        [Test]
        public void DownloadWithoutCoordinateForOneCase_ReturnsOneLineLess()
        {
            var sending = _db.MeningoSendings.First();
            sending.Patient.PostalCode = "SV-023-00000";
            _geonamesService.QueryCoordinateByPostalCode("SV-023-00000", Arg.Any<string>()).Returns((Geolocation.Coordinate?) null);
            var sut = CreateController();

            var lines = ExportLines(sut);

            lines.Should().HaveCount(_db.MeningoSendings.Count() - 1);
            lines.Should().NotContain(l => l.StartsWith($"{sending.MeningoPatientId};"));
        }

        // specs/023 FR-006, US5.1: the lookup receives the patient's "Postleitzahl" and "Wohnort"
        [Test]
        public void Download_QueriesCoordinateWithPatientPostalCodeAndCity()
        {
            var sending = _db.MeningoSendings.First();
            sending.Patient.PostalCode = "91301";
            sending.Patient.City = "Forchheim";
            var sut = CreateController();

            ExportLines(sut);

            _geonamesService.Received().QueryCoordinateByPostalCode("91301", "Forchheim");
        }

        // specs/023 US5.2 (spec 022 FR-005, FR-011): with the real lookup component the place whose name equals
        // "Wohnort" is chosen and its coordinates become columns 9 and 10
        [Test]
        public void DownloadWithGeonamesService_UsesPlaceMatchingCity()
        {
            foreach (var other in _db.MeningoSendings.Skip(1))
            {
                other.Deleted = true;
            }
            var sending = _db.MeningoSendings.First();
            sending.Patient.PostalCode = "06773";
            sending.Patient.City = "Gräfenhainichen";
            _geonamesService = new GeonamesService(_ => GeonamesAnswerFor06773);
            var sut = CreateController();

            var lines = ExportLines(sut);

            lines.Should().ContainSingle().Which.Should().EndWith(";51.728924;12.456513");
        }

        // specs/023 US6.2, FR-011: a "not found" answer of the service leaves out only that case; the file is delivered
        [Test]
        public void DownloadWithGeonamesNotFoundForOneCase_LeavesOutOnlyThatCase()
        {
            var first = _db.MeningoSendings.First();
            first.Patient.PostalCode = "SV-023-00000";
            _geonamesService = new GeonamesService(url =>
            {
                if (url.Contains("postalcode=SV-023-00000"))
                {
                    throw new System.Net.WebException();
                }
                return "{\"postalcodes\":[{\"placeName\":\"SV-023-Ort\",\"lat\":49.71754,\"lng\":11.05877}]}";
            });
            foreach (var sending in _db.MeningoSendings)
            {
                sending.Patient.City = string.Empty;
            }
            var sut = CreateController();

            var lines = ExportLines(sut);

            lines.Should().HaveCount(_db.MeningoSendings.Count() - 1);
            lines.Should().NotContain(l => l.StartsWith($"{first.MeningoPatientId};"));
        }

        // specs/023 FR-007, US4.2: with equal line length the case read first is kept
        [Test]
        public void DownloadWithTwoEqualLengthCasesOfSamePatient_KeepsTheCaseReadFirst()
        {
            var firstEntry = _db.MeningoSendings.First();
            SetupToInterpretAsSerogroup(firstEntry, MeningoSerogroupPcr.B);
            var secondEntry = _db.MeningoSendings.Skip(1).First();
            SetupToInterpretAsSerogroup(secondEntry, MeningoSerogroupPcr.C);
            secondEntry.MeningoPatientId = firstEntry.MeningoPatientId;
            secondEntry.Patient = firstEntry.Patient;
            secondEntry.SamplingDate = firstEntry.SamplingDate;
            secondEntry.ReceivingDate = firstEntry.ReceivingDate;
            var sut = CreateController();

            var lines = ExportLines(sut).Where(l => l.StartsWith($"{firstEntry.MeningoPatientId};")).ToList();

            lines.Should().ContainSingle().Which.Should().Match("*;B;22;14;5-5;*");
        }

        // specs/023 FR-009, US3.3, US3.5, US3.6: empty sampling date column, age from the receiving date,
        // point as decimal separator also under a German culture
        [Test]
        public void DownloadWithoutSamplingDate_WritesEmptyColumnAndAgeAtReceiving()
        {
            var firstEntry = _db.MeningoSendings.First();
            firstEntry.SamplingDate = null;
            firstEntry.ReceivingDate = new DateTime(2022, 01, 10);
            firstEntry.Patient.BirthDate = new DateTime(2012, 1, 1);
            var sut = CreateController();
            var previousCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            try
            {
                var lines = ExportLines(sut);

                lines.First().Should().Be("1;10.01.2022;;C;22;14;5-5;10;49.71754;11.05877");
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }

        // specs/023 FR-010, US3.1: CSV announced as text/csv, named after the server's current date
        [Test]
        public void Download_ReturnsCsvNamedAfterCurrentDate()
        {
            var sut = CreateController();
            var before = $"EpiscanGis_Export_{DateTime.Now:yyyyMMdd}.csv";

            var result = sut.Index(ValidToken).Should().BeOfType<FileContentResult>().Subject;

            var after = $"EpiscanGis_Export_{DateTime.Now:yyyyMMdd}.csv";
            result.ContentType.Should().Be("text/csv");
            result.FileDownloadName.Should().BeOneOf(before, after);
        }

        // specs/023 FR-010, US3.8: an empty file is delivered when no case qualifies
        [Test]
        public void DownloadWithoutQualifyingCase_ReturnsEmptyFile()
        {
            foreach (var sending in _db.MeningoSendings)
            {
                sending.SamplingLocation = MeningoSamplingLocation.EarSwab;
            }
            var sut = CreateController();

            var result = sut.Index(ValidToken).Should().BeOfType<FileContentResult>().Subject;

            result.FileContents.Should().BeEmpty();
            result.FileDownloadName.Should().StartWith("EpiscanGis_Export_");
        }

        // specs/023 FR-010, US2.7: lines in ascending order of patient number, whatever the stored order
        [Test]
        public void Download_WritesLinesInAscendingPatientNumberOrder()
        {
            var patientId = 100;
            foreach (var sending in _db.MeningoSendings)
            {
                sending.MeningoPatientId = patientId--;
            }
            var sut = CreateController();

            var patientNumbers = ExportLines(sut).Select(l => int.Parse(l.Split(';')[0])).ToList();

            patientNumbers.Should().HaveCount(_db.MeningoSendings.Count());
            patientNumbers.Should().BeInAscendingOrder();
        }

        // specs/023 FR-012, US6.4, US6.5, SC-006: no record is saved and no file is left in the temp folder
        [Test]
        public void Download_SavesNothingAndLeavesNoTemporaryFile()
        {
            var db = Substitute.For<IApplicationDbContext>();
            db.MeningoSendings.Returns(_db.MeningoSendings);
            var tempDirectory = Path.Combine(TemporaryDirectoryToStoreTestData, "temp");
            Directory.CreateDirectory(tempDirectory);
            var previousTmp = Environment.GetEnvironmentVariable("TMP");
            var previousTemp = Environment.GetEnvironmentVariable("TEMP");
            Environment.SetEnvironmentVariable("TMP", tempDirectory);
            Environment.SetEnvironmentVariable("TEMP", tempDirectory);
            try
            {
                var sut = new EpiscanGisExportController(db, _geonamesService);

                var result = sut.Index(ValidToken).Should().BeOfType<FileContentResult>().Subject;

                result.FileContents.Should().NotBeEmpty();
                Directory.GetFiles(tempDirectory).Should().BeEmpty();
                db.DidNotReceive().SaveChanges();
            }
            finally
            {
                Environment.SetEnvironmentVariable("TMP", previousTmp);
                Environment.SetEnvironmentVariable("TEMP", previousTemp);
            }
        }

        private string[] ExportLines(EpiscanGisExportController sut)
        {
            var content = sut.Index(ValidToken).Should().BeOfType<FileContentResult>().Subject.FileContents;
            var testFile = Path.Combine(TemporaryDirectoryToStoreTestData, "download.csv");
            File.WriteAllBytes(testFile, content);
            return File.ReadAllLines(testFile);
        }

        private const string GeonamesAnswerFor06773 =
            "{\"postalcodes\":[{\"lng\":12.444217,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"placeName\":\"Gossa\",\"lat\":51.669489}," +
            "{\"lng\":12.456513,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"placeName\":\"Gräfenhainichen\",\"lat\":51.728924}]}";

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
            isolate.MaldiTofVitek = UnspecificTestResult.NotDetermined;
            isolate.MaldiTofBiotyper = UnspecificTestResult.NotDetermined;
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