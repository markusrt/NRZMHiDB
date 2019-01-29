using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Models;
using Moq;
using NUnit.Framework;

namespace HaemophilusWeb.Views.Utils
{
    public class BootstrapEditorTests
    {
        [Test]
        public void TextEditorFor_SimpleProperty_CreatesBootstrapTextEditor()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.SimpleProperty, "Placeholder");

            textEditorHtml.ToHtmlString().Should().Match("*<label*for*SimpleProperty*");
            textEditorHtml.ToHtmlString().Should().Match("*<input*form-control*id=\"SimpleProperty\" name=\"SimpleProperty\"*");
            textEditorHtml.ToHtmlString().Should().Match("*placeholder=\"Placeholder\"*");
            
        }

        [Test]
        public void TextEditorFor_SimpleProperty_ShouldNotContainGlyphicons()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.SimpleProperty);

            textEditorHtml.ToHtmlString().Should().NotContain("input-group-addon");
            textEditorHtml.ToHtmlString().Should().NotContain("glyphicon glyphicon-star");
        }

        [Test]
        public void TextEditorFor_SimplePropertyWithPrefix_ShouldAddPrefix()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.SimpleProperty, prefix: "PF");

            textEditorHtml.ToHtmlString().Should().Match("*<span class*input-group-addon*>PF<*span>*");
            textEditorHtml.ToHtmlString().Should().Match("*<label*for*SimpleProperty*");
            textEditorHtml.ToHtmlString().Should().Match("*<input*form-control*id=\"SimpleProperty\" name=\"SimpleProperty\"*");
        }

        [Test]
        public void TextEditorFor_RequiredProperty_ShouldContainGlyphiconStar()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.RequiredProperty);

            textEditorHtml.ToHtmlString().Should().Contain("input-group-addon");
            textEditorHtml.ToHtmlString().Should().Contain("glyphicon glyphicon-star");
        }

        [Test]
        public void EnumRadioEditorFor_Enum_CreatesBootstrapRadioEditor()
        {
            var simpleModel = new SimpleModel {HibVaccination = YesNoUnknown.NotStated};
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(simpleModel));
            var enumRadioEditorHtml = helper.EnumRadioEditorFor(m => m.HibVaccination);

            enumRadioEditorHtml.ToHtmlString().Should().Be("<div class=\"form-group\"><label class=\"col-sm-2 control-label\" for=\"HibVaccination\">Hib-Impfung</label><div class=\"col-sm-5\"><div><div class=\"btn-group\" data-toggle=\"buttons\"><label class=\"btn btn-default \"><input id=\"HibVaccination_No\" name=\"HibVaccination\" type=\"radio\" value=\"No\" /> Nein</label><label class=\"btn btn-default \"><input id=\"HibVaccination_Yes\" name=\"HibVaccination\" type=\"radio\" value=\"Yes\" /> Ja</label><label class=\"btn btn-default active\"><input checked=\"checked\" id=\"HibVaccination_NotStated\" name=\"HibVaccination\" type=\"radio\" value=\"NotStated\" /> keine Angabe</label><label class=\"btn btn-default \"><input id=\"HibVaccination_Unknown\" name=\"HibVaccination\" type=\"radio\" value=\"Unknown\" /> Unbekannt</label></div></div></div></div>");
        }

        [Test]
        public void EnumRadioEditorFor_FlagsEnum_CreatesBootstrapRadioEditor()
        {
            var simpleModel = new SimpleModel();
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(simpleModel));
            var enumRadioEditorHtml = helper.EnumRadioEditorFor(m => m.ClinicalInformation, "col-lg-9");

            enumRadioEditorHtml.ToHtmlString().Should().Be("<div class=\"form-group\"><label class=\"col-sm-2 control-label\" for=\"ClinicalInformation\">Klinische Angaben</label><div class=\"col-lg-9\"><div><div class=\"btn-group\" data-toggle=\"buttons\"><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_NotAvailable\" name=\"ClinicalInformation\" value=\"NotAvailable\" /> k.A.</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Meningitis\" name=\"ClinicalInformation\" value=\"Meningitis\" /> Meningitis</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Sepsis\" name=\"ClinicalInformation\" value=\"Sepsis\" /> Sepsis</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Pneumonia\" name=\"ClinicalInformation\" value=\"Pneumonia\" /> Pneumonie</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Other\" name=\"ClinicalInformation\" value=\"Other\" /> Andere</label></div></div></div></div>");
        }
    }
}