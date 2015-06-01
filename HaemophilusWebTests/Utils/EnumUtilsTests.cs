using System;
using System.IO;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestDoubles;
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

        private readonly UtilsTest? ZeroAsNullable = UtilsTest.Zero;

        [TestCase(0)]
        [TestCase("Zero")]
        [TestCase(UtilsTest.Zero)]
        public void GetEnumDescription_Attribute_ReturnsDescription(object value)
        {
            EnumUtils.GetEnumDescription<UtilsTest>(value).Should().Be("Null");
        }

        [TestCase(UtilsTest.Zero, "Null")]
        [TestCase(1, "Eins")]
        public void GetEnumDescription_NullableType_ReturnsDescription(object value, string expected)
        {
            EnumUtils.GetEnumDescription<UtilsTest?>(value).Should().Be(expected);
        }

        [Test]
        public void GetEnumDescription_NoAttribute_ReturnsEnumName()
        {
            EnumUtils.GetEnumDescription<UtilsTest>("Two").Should().Be("Two");
        }

        [Test]
        [TestCase(3, "3")]
        [TestCase("Three", "Three")]
        public void GetEnumDescription_NoEnumEntry_ReturnsInput(object value, string expected)
        {
            EnumUtils.GetEnumDescription<UtilsTest>(value).Should().Be(expected);
        }
    }
}