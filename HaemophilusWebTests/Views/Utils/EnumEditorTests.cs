using FluentAssertions;
using HaemophilusWeb.TestDoubles;
using NUnit.Framework;

namespace HaemophilusWeb.Views.Utils
{
    public class EnumEditorTests
    {
        [TestCase(null, "")]
        [TestCase(UtilsTest.One, "Eins")]
        public void GetEnumDescription_ValidEntry_ReturnsCorrectString(UtilsTest? enumValue,
            string expectedStringRepresentation)
        {
            EnumEditor.GetEnumDescription(enumValue).Should().Be(expectedStringRepresentation);
        }

        [TestCase(null, "")]
        [TestCase(FlagsEnum.Zero, "")]
        [TestCase(FlagsEnum.One | FlagsEnum.Two, "Eins, Zwei")]
        [TestCase(FlagsEnum.Two | FlagsEnum.Four, "Zwei, Four")]
        public void GetEnumDescription_ValidFlagEntry_ReturnsCorrectString(FlagsEnum? enumValue,
            string expectedStringRepresentation)
        {
            EnumEditor.GetEnumDescription(enumValue).Should().Be(expectedStringRepresentation);
        }
    }
}