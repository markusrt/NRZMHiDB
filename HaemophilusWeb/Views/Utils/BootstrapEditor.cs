using HaemophilusWeb.Utils;
using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HaemophilusWeb.Views.Utils
{
    public static class BootstrapEditor
    {
        private const string FormGroupTemplate = "<div class=\"row mb-3\">{0}{1}</div>";

        private const string FormGroupTemplateWithId = "<div id=\"{2}\" class=\"row mb-3\">{0}{1}</div>";

        private const string ColSm5 = "col-sm-5";

        private const string DivSmX = "<div class=\"{0}\">{1}{2}</div>";

        private const string DivSmXStatic = "<div class=\"{0} form-control-plaintext\">{1}{2}</div>";

        private const string DivSmFive = "<div class=\"col-sm-5\">{0}{1}</div>";

        private const string RequiredTemplate = "<div class=\"input-group\">{0}<span class=\"input-group-text\"><span class=\"bi bi-star-fill\"></span></span></div>";

        private const string IconTemplate = "<div class=\"input-group\">{0}<span class=\"input-group-text\"><span class=\"bi {1}\"></span></span></div>";

        private const string PrefixTemplate =   "<div class=\"input-group\"><span class=\"input-group-text\">{1}</span>{0}</div>";

        // Tempus Dominus 6 markup: the input-group carries the picker id and the toggle/target data-td-* hooks.
        private const string DateTemplate =
            "<div class=\"input-group\" id=\"{1}\" data-td-target-input=\"nearest\" data-td-target-toggle=\"nearest\">" +
            "{0}" +
            "<span class=\"input-group-text\" data-td-target=\"#{1}\" data-td-toggle=\"datetimepicker\"><span class=\"bi bi-calendar\"></span></span>" +
            "</div>";

        public static MvcHtmlString TextEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string placeholder = null, string prefix = null, string id = null, string smXClass = ColSm5, string icon=null)
        {
            var label = htmlHelper.LabelFor(expression);
            var textBox = htmlHelper.TextBoxFor(expression, new {placeholder, @class = $"form-control"}).ToHtmlString();
            var validationHtml = GetValidationHtml(htmlHelper, expression);
            
            //TODO: IsRequired does not work with fluent validations. I.e. implement FluentValidation based ModelMetadata if the view should render a required state 
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (modelMetadata.IsRequired)
            {
                textBox = string.Format(RequiredTemplate, textBox);
            }
            else if (!string.IsNullOrEmpty(icon))
            {
                textBox = string.Format(IconTemplate, textBox, icon);
            }
            else if (!string.IsNullOrEmpty(prefix))
            {
                textBox = string.Format(PrefixTemplate, textBox, prefix);
            }

            var textBoxHtml = string.Format(DivSmX, smXClass, textBox, validationHtml);

            return CreateFormGroup(label, textBoxHtml, id);
        }

        public static MvcHtmlString DateEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string additionalClass = "", string id = null)
        {
            var label = htmlHelper.LabelFor(expression);
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var model = modelMetadata.Model;
            var pickerId = $"{modelMetadata.PropertyName}_picker";
            htmlHelper.EnableClientValidation(false);
            var textBox = htmlHelper.TextBoxFor(expression, new
            {
                @class = "form-control datepicker " + additionalClass,
                Value = model == null ? "" : $"{model:dd.MM.yyyy}",
                data_td_target = "#" + pickerId,
                autocomplete = "off"
            }).ToHtmlString();
            htmlHelper.EnableClientValidation(true);
            var validationHtml = GetValidationHtml(htmlHelper, expression);

            var textBoxHtml = string.Format(DivSmFive, string.Format(DateTemplate, textBox, pickerId), validationHtml);

            return CreateFormGroup(label, textBoxHtml, id);
        }


        public static MvcHtmlString EnumRadioEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string radioColSmXClass = ColSm5, string id = null, string suffix = null)
        {
            var label = htmlHelper.LabelFor(expression);
            var enumRadioButton = htmlHelper.EnumRadioButtonFor(expression, suffix:suffix).ToHtmlString();

            var enumRadioEditor = string.Format(DivSmX, radioColSmXClass, enumRadioButton, string.Empty);

            return CreateFormGroup(label, enumRadioEditor, id);
        }

        public static MvcHtmlString ReadonlyFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string smXClass = ColSm5, string suffix = "")
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var label = htmlHelper.LabelFor(expression);
            var display = htmlHelper.DisplayFor(expression).ToHtmlString();
            if (modelMetadata.ModelType.IsEnum)
            {
                display = EnumUtils.GetEnumDescription(modelMetadata.ModelType, modelMetadata.Model);
            }
            var hidden = htmlHelper.HiddenFor(expression).ToHtmlString();
            var readonlyField = string.Format(DivSmXStatic, smXClass, display+suffix, hidden);

            return CreateFormGroup(label, readonlyField);
        }

        private static MvcHtmlString CreateFormGroup(string labelHtml, string elementHtml, string id = null)
        {
            return id == null
                ? MvcHtmlString.Create(string.Format(FormGroupTemplate, labelHtml, elementHtml))
                : MvcHtmlString.Create(string.Format(FormGroupTemplateWithId, labelHtml, elementHtml, id));
        }

        private static string LabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return htmlHelper.LabelFor(expression, new { @class = "col-sm-2 col-form-label text-end fw-semibold" }).ToHtmlString();
        }

        private static string GetValidationHtml<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var validationMessage = htmlHelper.ValidationMessageFor(expression);
            if (validationMessage == null)
            {
                return string.Empty;
            }
            return validationMessage.ToHtmlString();
        }
    }
}