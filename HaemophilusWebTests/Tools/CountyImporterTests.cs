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
    public class CountyImporterTests : ITempDirectoryTest
    {
        private const string TestData =
            "102014123101          Schleswig-Holstein                                Kiel                                                                                                                                                \n" +
            "402014103101001       Flensburg, Stadt                                  Flensburg                                         41                                                                                                \n" +
            "502014123101001   0000Flensburg, Stadt                                                                                    50                                                                                                \n" +
            "6020141231010010000000Flensburg, Stadt                                                                                    61    000000056740000008397100000041322    24937*****  2115111211901001                           \n" +
            "402014123101002       Kiel, Landeshauptstadt                            Kiel                                              41                                                                                                \n" +
            "502014123101002   0000Kiel, Landeshauptstadt                                                                              50                                                                                                \n" +
            "6020141231010020000000Kiel, Landeshauptstadt                                                                              61    000000118650000024153300000116951    24103*****  2119151713101005                           \n" +
            "402014013101003       Lübeck, Hansestadt                                Lübeck                                            41                                                                                                ";

        public string TemporaryDirectoryToStoreTestData { get; set; }

        public string TestFile
        {
            get { return Path.Combine(TemporaryDirectoryToStoreTestData, "import.asc"); }
        }

        [Test]
        public void Ctor_DoesNotThrowException()
        {
            CreateImporter();
        }

        private CountyImporter CreateImporter()
        {
            File.WriteAllText(TestFile, TestData, Encoding.GetEncoding(1252));
            return new CountyImporter(TestFile);
        }

        [Test]
        public void LoadCounties_ReturnsCountiesWithCorrectNames()
        {
            var counties = LoadCounties();

            var expectedNames = new List<string> {"Flensburg, Stadt", "Kiel, Landeshauptstadt", "Lübeck, Hansestadt"};
            counties.Select(c => c.Name).Should().ContainInOrder(expectedNames);
        }

        [Test]
        public void LoadCounties_ReturnsCountiesWithCorrectValidSince()
        {
            var counties = LoadCounties();

            var expectedDates = new List<DateTime>
            {
                new DateTime(2014, 10, 31),
                new DateTime(2014, 12, 31),
                new DateTime(2014, 1, 31)
            };
            counties.Select(c => c.ValidSince).Should().ContainInOrder(expectedDates);
        }

        [Test]
        public void LoadCounties_ReturnsCountiesWithCorrectNumber()
        {
            var counties = LoadCounties();

            var expectedNumbers = new List<string> {"01001", "01002", "01003"};
            counties.Select(c => c.CountyNumber).Should().ContainInOrder(expectedNumbers);
        }

        // specs/025 FR-010, US2.3: only lines of record type "40" become counties; state, association and
        // municipality lines are skipped
        [Test]
        public void LoadCounties_ImportsOnlyCountyRecordLines()
        {
            var counties = LoadCounties().ToList();

            counties.Should().HaveCount(3);
            counties.Select(c => c.Name).Should().Equal("Flensburg, Stadt", "Kiel, Landeshauptstadt", "Lübeck, Hansestadt");
        }

        // specs/025 FR-010, US2.2: the validity date is read at midnight
        [Test]
        public void LoadCounties_ValidSinceHasNoTimeOfDay()
        {
            var counties = LoadCounties();

            counties.Select(c => c.ValidSince.TimeOfDay).Should().OnlyContain(t => t == TimeSpan.Zero);
        }

        // specs/025 FR-011, US2.6: a county line shorter than the name field makes the whole import fail
        [Test]
        public void LoadCounties_CountyLineShorterThanNameField_Throws()
        {
            var importer = CreateImporter(
                "402015010199101       SV-025-Gut                                        Ort\n" +
                "4020150101991020000000SV-025-Kurz");

            Action load = () => importer.LoadCounties().ToList();

            load.Should().Throw<ArgumentOutOfRangeException>();
        }

        // specs/025 FR-011, US2.6: a validity field that is not eight digits makes the whole import fail
        [Test]
        public void LoadCounties_ValidityNotEightDigits_ThrowsFormatException()
        {
            var importer = CreateImporter(
                "402015AB0199201       SV-025-Datum                                      Ort                               ");

            Action load = () => importer.LoadCounties().ToList();

            load.Should().Throw<FormatException>();
        }

        // specs/025 FR-010, edge case "File format assumptions": a file without county lines imports nothing
        [Test]
        public void LoadCounties_FileWithoutCountyLines_ReturnsNothing()
        {
            var importer = CreateImporter("Satzart;Datum;Schluessel;Name\nKreis;20150101;99301;SV-025-CSV");

            importer.LoadCounties().Should().BeEmpty();
        }

        private CountyImporter CreateImporter(string content)
        {
            File.WriteAllText(TestFile, content, Encoding.GetEncoding(1252));
            return new CountyImporter(TestFile);
        }

        private IEnumerable<County> LoadCounties()
        {
            var importer = CreateImporter();
            var counties = importer.LoadCounties();
            return counties;
        }
    }
}