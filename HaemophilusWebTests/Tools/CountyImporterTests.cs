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

        private IEnumerable<County> LoadCounties()
        {
            var importer = CreateImporter();
            var counties = importer.LoadCounties();
            return counties;
        }
    }
}