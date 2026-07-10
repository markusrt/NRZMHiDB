using System.Web.Mvc;
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



        [Test]
        public void EnumRadioButtonFor_FlagsEnum_UnboxesNullable()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var enumRadioEditorHtml = helper.EnumRadioButtonFor(m => m.GrowthType);

            enumRadioEditorHtml.ToHtmlString().Should().NotContain("GrowthType_None");
            enumRadioEditorHtml.ToHtmlString().Should().Contain("<input type=\"checkbox\" class=\"btn-check\" id=\"GrowthType_GrowthOnBlood\" name=\"GrowthType\" value=\"GrowthOnBlood\" autocomplete=\"off\" /><label class=\"btn btn-outline-secondary\" for=\"GrowthType_GrowthOnBlood\">Wachstum auf Blut</label>");
        }
    }
}