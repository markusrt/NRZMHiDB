using System.IO;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class EnumUtilsTests
    {
        [Test]
        [TestCase("Sepsis", ClinicalInformation.Sepsis)]
        [TestCase("Sepsis,,Meningitis", ClinicalInformation.Sepsis | ClinicalInformation.Meningitis)]
        [TestCase("", ClinicalInformation.None)]
        [TestCase(null, ClinicalInformation.None)]
        public void ParseCommaSeperatedListOfNamesAsFlagsEnum_ParsesCorrectly(string commaSeperatedList, ClinicalInformation expectedResult)
        {
            var result = EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<ClinicalInformation>(commaSeperatedList);

            result.Should().Be(expectedResult);
        }
    }
}