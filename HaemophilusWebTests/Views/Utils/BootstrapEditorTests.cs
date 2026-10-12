using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
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
            var simpleModel = new SimpleModel {HibVaccination = VaccinationStatus.NotStated};
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(simpleModel));
            var enumRadioEditorHtml = helper.EnumRadioEditorFor(m => m.HibVaccination, suffix: "test");

            enumRadioEditorHtml.ToHtmlString().Should().Be($"<div class=\"form-group\"><label class=\"col-sm-2 control-label\" for=\"HibVaccination\">Hib-Impfung</label><div class=\"col-sm-5\"><div><div class=\"btn-group\" data-toggle=\"buttons\"><label class=\"btn btn-default \"><input id=\"HibVaccination_No\" name=\"HibVaccination\" type=\"radio\" value=\"No\" /> Nein</label><label class=\"btn btn-default \"><input id=\"HibVaccination_Yes\" name=\"HibVaccination\" type=\"radio\" value=\"Yes\" /> Ja</label><label class=\"btn btn-default active\"><input checked=\"checked\" id=\"HibVaccination_NotStated\" name=\"HibVaccination\" type=\"radio\" value=\"NotStated\" /> keine Angabe</label><label class=\"btn btn-default \"><input id=\"HibVaccination_Unknown\" name=\"HibVaccination\" type=\"radio\" value=\"Unknown\" /> Unbekannt</label><label class=\"btn btn-default \"><input id=\"HibVaccination_Incomplete\" name=\"HibVaccination\" type=\"radio\" value=\"Incomplete\" /> nicht vollständig</label></div><span style=\"margin-left:0.5em\" class=\"badge badge-light\">test</span></div></div></div>");
        }

        [Test]
        public void EnumRadioEditorFor_FlagsEnum_CreatesBootstrapRadioEditor()
        {
            var simpleModel = new SimpleModel();
            var helper = TestUtils.CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(simpleModel));
            var enumRadioEditorHtml = helper.EnumRadioEditorFor(m => m.ClinicalInformation, "col-lg-9");

            enumRadioEditorHtml.ToHtmlString().Should().Be("<div class=\"form-group\"><label class=\"col-sm-2 control-label\" for=\"ClinicalInformation\">Klinische Angaben</label><div class=\"col-lg-9\"><div><div class=\"btn-group\" data-toggle=\"buttons\"><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_NotAvailable\" name=\"ClinicalInformation\" value=\"NotAvailable\" /> k.A.</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Meningitis\" name=\"ClinicalInformation\" value=\"Meningitis\" /> Meningitis</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Sepsis\" name=\"ClinicalInformation\" value=\"Sepsis\" /> Sepsis</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Pneumonia\" name=\"ClinicalInformation\" value=\"Pneumonia\" /> Pneumonie</label><label class=\"btn btn-default \"><input type=\"checkbox\" id=\"ClinicalInformation_Other\" name=\"ClinicalInformation\" value=\"Other\" /> Andere</label></div></div></div></div>");
        }

        [Test]
        public void MatchWithConfidenceEditorFor_IsolateViewModel_MatchesGoldMasterExactly()
        {
            var possibleMatches = new[] { "Haemophilus influenzae", "Haemophilus parainfluenzae", "Haemophilus haemolyticus" };
            var model = new IsolateViewModel
            {
                MaldiTofBiotyperBestMatch = "Haemophilus influenzae",
                MaldiTofBiotyperMatchConfidence = 2.15
            };
            var viewData = new ViewDataDictionary(model)
            {
                ["PossibleMaldiTofBiotyperBestMatches"] = possibleMatches
            };
            var helper = TestUtils.CreateHtmlHelper<IsolateViewModel>(viewData);

            var goldMasterHtml = RenderOriginalMaldiTofBiotyperForm(
                helper,
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                possibleMatches);

            var helperHtml = helper.MatchWithConfidenceEditorFor(
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                id: "MaldiTofMatchConfidenceForm",
                datalistId: "maldiTofBiotyperBestMatchAutocomplete",
                autocompleteList: possibleMatches);

            helperHtml.ToHtmlString().Should().Be(goldMasterHtml);
        }

        [Test]
        public void MatchWithConfidenceEditorFor_MeningoIsolateViewModel_MatchesGoldMasterExactly()
        {
            var possibleMatches = new[] { "Neisseria meningitidis", "Neisseria gonorrhoeae", "Neisseria lactamica" };
            var model = new MeningoIsolateViewModel
            {
                MaldiTofBiotyperBestMatch = "Neisseria meningitidis",
                MaldiTofBiotyperMatchConfidence = 2.34
            };
            var viewData = new ViewDataDictionary(model)
            {
                ["PossibleMaldiTofBiotyperBestMatches"] = possibleMatches
            };
            var helper = TestUtils.CreateHtmlHelper<MeningoIsolateViewModel>(viewData);

            var goldMasterHtml = RenderOriginalMaldiTofBiotyperForm(
                helper,
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                possibleMatches);

            var helperHtml = helper.MatchWithConfidenceEditorFor(
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                id: "MaldiTofMatchConfidenceForm",
                datalistId: "maldiTofBiotyperBestMatchAutocomplete",
                autocompleteList: possibleMatches);

            helperHtml.ToHtmlString().Should().Be(goldMasterHtml);
        }

        [Test]
        public void MatchWithConfidenceEditorFor_IsolateViewModel_EmptyModel_MatchesGoldMasterExactly()
        {
            var possibleMatches = new[] { "Haemophilus influenzae", "Haemophilus parainfluenzae" };
            var model = new IsolateViewModel();
            var viewData = new ViewDataDictionary(model)
            {
                ["PossibleMaldiTofBiotyperBestMatches"] = possibleMatches
            };
            var helper = TestUtils.CreateHtmlHelper<IsolateViewModel>(viewData);

            var goldMasterHtml = RenderOriginalMaldiTofBiotyperForm(
                helper,
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                possibleMatches);

            var helperHtml = helper.MatchWithConfidenceEditorFor(
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                id: "MaldiTofMatchConfidenceForm",
                datalistId: "maldiTofBiotyperBestMatchAutocomplete",
                autocompleteList: possibleMatches);

            helperHtml.ToHtmlString().Should().Be(goldMasterHtml);
        }

        [Test]
        public void MatchWithConfidenceEditorFor_ResolvesAutocompleteFromViewDataAutomatically()
        {
            var possibleMatches = new[] { "Haemophilus influenzae" };
            var model = new IsolateViewModel();
            var viewData = new ViewDataDictionary(model)
            {
                ["PossibleMaldiTofBiotyperBestMatches"] = possibleMatches
            };
            var helper = TestUtils.CreateHtmlHelper<IsolateViewModel>(viewData);

            var helperHtml = helper.MatchWithConfidenceEditorFor(
                m => m.MaldiTofBiotyperBestMatch,
                m => m.MaldiTofBiotyperMatchConfidence,
                id: "MaldiTofMatchConfidenceForm");

            helperHtml.ToHtmlString().Should().Contain("<datalist id=\"maldiTofBiotyperBestMatchAutocomplete\"><option value=\"Haemophilus influenzae\" /></datalist>");
            helperHtml.ToHtmlString().Should().Contain("list=\"maldiTofBiotyperBestMatchAutocomplete\"");
        }

        [Test]
        public void MatchWithConfidenceEditorFor_RibosomalRna16S_IsolateViewModel_MatchesOriginalMarkup()
        {
            var model = new IsolateViewModel
            {
                RibosomalRna16SBestMatch = "Haemophilus influenzae",
                RibosomalRna16SMatchInPercent = 99.5
            };
            var helper = TestUtils.CreateHtmlHelper<IsolateViewModel>(new ViewDataDictionary(model));

            var originalHtml = RenderOriginalRibosomalRna16SForm(
                helper,
                m => m.RibosomalRna16SBestMatch,
                m => m.RibosomalRna16SMatchInPercent);

            var helperHtml = helper.MatchWithConfidenceEditorFor(
                m => m.RibosomalRna16SBestMatch,
                m => m.RibosomalRna16SMatchInPercent,
                confidencePlaceholder: "in Prozent",
                addon: "%",
                id: "RibosomalRna16SMatchInPercentForm");

            helperHtml.ToHtmlString().Should().Be(originalHtml);
        }

        [Test]
        public void MatchWithConfidenceEditorFor_RibosomalRna16S_MeningoIsolateViewModel_MatchesOriginalMarkup()
        {
            var model = new MeningoIsolateViewModel
            {
                RibosomalRna16SBestMatch = "Neisseria meningitidis",
                RibosomalRna16SMatchInPercent = 98.7
            };
            var helper = TestUtils.CreateHtmlHelper<MeningoIsolateViewModel>(new ViewDataDictionary(model));

            var originalHtml = RenderOriginalRibosomalRna16SForm(
                helper,
                m => m.RibosomalRna16SBestMatch,
                m => m.RibosomalRna16SMatchInPercent);

            var helperHtml = helper.MatchWithConfidenceEditorFor(
                m => m.RibosomalRna16SBestMatch,
                m => m.RibosomalRna16SMatchInPercent,
                confidencePlaceholder: "in Prozent",
                addon: "%",
                id: "RibosomalRna16SMatchInPercentForm");

            helperHtml.ToHtmlString().Should().Be(originalHtml);
        }

        private static string RenderOriginalMaldiTofBiotyperForm<TModel>(
            HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> bestMatchExpr,
            Expression<Func<TModel, double?>> confidenceExpr,
            IEnumerable<string> possibleMatches)
        {
            var label = htmlHelper.LabelFor(confidenceExpr, new { @class = "col-sm-2 control-label" });
            var textBoxBestMatch = htmlHelper.TextBoxFor(bestMatchExpr, new { @class = "form-control", placeholder = "Beste Übereinstimmung", @list = "maldiTofBiotyperBestMatchAutocomplete", @autocomplete = "off" });
            var valBestMatch = htmlHelper.ValidationMessageFor(bestMatchExpr);

            var datalist = new StringBuilder("<datalist id=\"maldiTofBiotyperBestMatchAutocomplete\">");
            if (possibleMatches != null)
            {
                foreach (var item in possibleMatches)
                {
                    datalist.Append($"<option value=\"{item}\" />");
                }
            }
            datalist.Append("</datalist>");

            var textBoxConfidence = htmlHelper.TextBoxFor(confidenceExpr, new { @class = "form-control", @placeholder = "Konfidenzwert" });
            var valConfidence = htmlHelper.ValidationMessageFor(confidenceExpr);

            return $"<div id=\"MaldiTofMatchConfidenceForm\" class=\"form-group\">" +
                   $"{label}" +
                   $"<div class=\"col-sm-4\">{textBoxBestMatch}{datalist}{valBestMatch}</div>" +
                   $"<div class=\"col-sm-3\"><div class=\"input-group\">{textBoxConfidence}<span class=\"input-group-addon\">0,0</span></div>{valConfidence}</div>" +
                   $"</div>";
        }

        private static string RenderOriginalRibosomalRna16SForm<TModel>(
            HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> bestMatchExpr,
            Expression<Func<TModel, double?>> confidenceExpr)
        {
            var label = htmlHelper.LabelFor(confidenceExpr, new { @class = "col-sm-2 control-label" });
            var textBoxBestMatch = htmlHelper.TextBoxFor(bestMatchExpr, new { @class = "form-control", placeholder = "Beste Übereinstimmung" });
            var valBestMatch = htmlHelper.ValidationMessageFor(bestMatchExpr);

            var textBoxConfidence = htmlHelper.TextBoxFor(confidenceExpr, new { @class = "form-control", placeholder = "in Prozent" });
            var valConfidence = htmlHelper.ValidationMessageFor(confidenceExpr);

            return $"<div id=\"RibosomalRna16SMatchInPercentForm\" class=\"form-group\">" +
                   $"{label}" +
                   $"<div class=\"col-sm-4\">{textBoxBestMatch}{valBestMatch}</div>" +
                   $"<div class=\"col-sm-3\"><div class=\"input-group\">{textBoxConfidence}<span class=\"input-group-addon\">%</span></div>{valConfidence}</div>" +
                   $"</div>";
        }
    }
}