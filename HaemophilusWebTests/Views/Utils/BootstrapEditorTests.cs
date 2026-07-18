using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Models;
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

            textEditorHtml.ToHtmlString().Should().NotContain("input-group-text");
            textEditorHtml.ToHtmlString().Should().NotContain("bi bi-star-fill");
        }

        [Test]
        public void TextEditorFor_SimplePropertyWithPrefix_ShouldAddPrefix()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.SimpleProperty, prefix: "PF");

            textEditorHtml.ToHtmlString().Should().Match("*<span class*input-group-text*>PF<*span>*");
            textEditorHtml.ToHtmlString().Should().Match("*<label*for*SimpleProperty*");
            textEditorHtml.ToHtmlString().Should().Match("*<input*form-control*id=\"SimpleProperty\" name=\"SimpleProperty\"*");
        }

        [Test]
        public void TextEditorFor_RequiredProperty_ShouldContainRequiredIcon()
        {
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.RequiredProperty);

            textEditorHtml.ToHtmlString().Should().Contain("input-group-text");
            textEditorHtml.ToHtmlString().Should().Contain("bi bi-star-fill");
        }

        [Test]
        public void EnumRadioEditorFor_Enum_CreatesBootstrapRadioEditor()
        {
            var simpleModel = new SimpleModel {HibVaccination = VaccinationStatus.NotStated};
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(simpleModel));
            var enumRadioEditorHtml = helper.EnumRadioEditorFor(m => m.HibVaccination, suffix: "test");

            var html = enumRadioEditorHtml.ToHtmlString();
            html.Should().StartWith("<div class=\"row mb-3\"><label class=\"col-sm-2 col-form-label text-end fw-semibold\" for=\"HibVaccination\">Hib-Impfung</label><div class=\"col-sm-5\">");
            html.Should().Contain("<div class=\"btn-group flex-wrap\" role=\"group\">");
            html.Should().Contain("class=\"btn-check\"");
            html.Should().Contain("<label class=\"btn btn-outline-secondary\" for=\"HibVaccination_NotStated\">keine Angabe</label>");
            html.Should().Contain("checked=\"checked\"");
            html.Should().Contain("<span style=\"margin-left:0.5em\" class=\"badge text-bg-light\">test</span>");
            html.Should().NotContain("data-toggle");
            html.Should().NotContain("btn-default");
        }

        [Test]
        public void EnumRadioEditorFor_FlagsEnum_CreatesBootstrapRadioEditor()
        {
            var simpleModel = new SimpleModel();
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(simpleModel));
            var enumRadioEditorHtml = helper.EnumRadioEditorFor(m => m.ClinicalInformation, "col-lg-9");

            var html = enumRadioEditorHtml.ToHtmlString();
            html.Should().StartWith("<div class=\"row mb-3\"><label class=\"col-sm-2 col-form-label text-end fw-semibold\" for=\"ClinicalInformation\">Klinische Angaben</label><div class=\"col-lg-9\">");
            html.Should().Contain("<div class=\"btn-group flex-wrap\" role=\"group\">");
            html.Should().Contain("<input type=\"checkbox\" class=\"btn-check\" id=\"ClinicalInformation_Meningitis\" name=\"ClinicalInformation\" value=\"Meningitis\" autocomplete=\"off\" />");
            html.Should().Contain("<label class=\"btn btn-outline-secondary\" for=\"ClinicalInformation_Meningitis\">Meningitis</label>");
            html.Should().NotContain("data-toggle");
            html.Should().NotContain("btn-default");
        }
    }
}