using System;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestDoubles;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    [TestFixture]
    public class EnumSerializerTests
    {
        [Test]
        [Sequential]
        public void DeserializeEnumStrict_InvalidString_ThrowsException(
            [Values("Apple", "12345")] string value)
        {
            Assert.Throws<Exception>(() => EnumSerializer.DeserializeEnumStrict<UtilsTest>(value));
        }

        [Test]
        public void DeserializeEnumStrict_ValidString_ReturnsCorrectValue()
        {
            const UtilsTest definedEnumValue = UtilsTest.One;
            var inputString = definedEnumValue.ToString();

            EnumSerializer.DeserializeEnumStrict<UtilsTest>(inputString).Should().Be(definedEnumValue);
            EnumSerializer.DeserializeEnumStrict<UtilsTest>(inputString.ToLower()).Should().Be(definedEnumValue);
        }

        [Test]
        public void DeserializeEnumStrict_ValidFlagString_ReturnsCorrectValue()
        {
            const ClinicalInformation definedEnumValue = ClinicalInformation.Meningitis | ClinicalInformation.Pneumonia;
            var inputString = definedEnumValue.ToString();

            EnumSerializer.DeserializeEnumStrict<ClinicalInformation>(inputString).Should().Be(definedEnumValue);
            EnumSerializer.DeserializeEnumStrict<ClinicalInformation>(inputString.ToLower())
                .Should()
                .Be(definedEnumValue);
        }

        [Test]
        [TestCase("Eins", UtilsTest.One)]
        [TestCase("E", UtilsTest.One)]
        [TestCase("Ei", UtilsTest.One)]
        public void DeserializeEnumStrict_ValidDisplayString_ReturnsCorrectValue(string inputString,
            UtilsTest definedEnumValue)
        {
            EnumSerializer.DeserializeEnumStrict<UtilsTest>(inputString).Should().Be(definedEnumValue);
            EnumSerializer.DeserializeEnumStrict<UtilsTest>(inputString.ToLower()).Should().Be(definedEnumValue);
        }
    }
}