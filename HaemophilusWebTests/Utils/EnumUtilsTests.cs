using System;
using System.IO;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    [TestFixture]
    public class EnumUtilsTests
    {
        [Test]
        [TestCase("Sepsis", ClinicalInformation.Sepsis)]
        [TestCase("Sepsis,,Meningitis", ClinicalInformation.Sepsis | ClinicalInformation.Meningitis)]
        [TestCase("", ClinicalInformation.None)]
        [TestCase(null, ClinicalInformation.None)]
        public void ParseCommaSeperatedListOfNamesAsFlagsEnum_ParsesCorrectly(string commaSeperatedList,
            ClinicalInformation expectedResult)
        {
            var result = EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<ClinicalInformation>(commaSeperatedList);

            result.Should().Be(expectedResult);
        }

        private enum UtilsTest
        {
            Zero = 0,
            [Mock] One = 1,
            Two = 2
        }

        private class MockAttribute : Attribute
        {
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void AllEnumValues_NotAnEnum_ThrowsException()
        {
            EnumUtils.AllEnumValues<int>();
        }

        [Test]
        public void AllEnumValues_ValidEnum_ReturnsAllValues()
        {
            var values = EnumUtils.AllEnumValues<UtilsTest>();

            values.Should().BeEquivalentTo(new[] {UtilsTest.Zero, UtilsTest.One, UtilsTest.Two});
        }

        [Test]
        public void FirstAttribute_AttributeDoesNotExist_ReturnsNull()
        {
            var attribute = UtilsTest.Zero.FirstAttribute<MockAttribute>();

            attribute.Should().BeNull();
        }

        [Test]
        public void FirstAttribute_AttributeExists_DoesNotReturnNull()
        {
            var attribute = UtilsTest.One.FirstAttribute<MockAttribute>();

            attribute.Should().NotBeNull();
            attribute.GetType().ShouldBeEquivalentTo(typeof (MockAttribute));
        }

        [Test]
        public void IsDefinedEnumValue_WhenPassedACombinationWhichAlsoExistsAsExplicitEnumEntry_ReturnsTrue()
        {
            var definedFlagValueOfCombination = (FileAccess.Read | FileAccess.Write).IsDefinedEnumValue();
            var definedFlagValueOfequivalentSingleEntry = FileAccess.ReadWrite.IsDefinedEnumValue();

            definedFlagValueOfCombination.Should().BeTrue();
            definedFlagValueOfequivalentSingleEntry.Should().BeTrue();
        }
    }
}