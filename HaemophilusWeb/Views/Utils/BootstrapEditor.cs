using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HaemophilusWeb.Views.Utils
{
    public static class BootstrapEditor
    {
        private const string FormGroupTemplate = "<div class=\"form-group row\">{0}{1}</div>";

        private const string FormGroupTemplateWithId = "<div id=\"{2}\" class=\"form-group row\">{0}{1}</div>";

        private const string ColSm5 = "col-sm-5";

        private const string DivSmX = "<div class=\"{0}\">{1}{2}</div>";

        private const string DivSmXStatic = "<div class=\"{0} form-control-plaintext\">{1}{2}</div>";
        
        private const string DivSmFive = "<div class=\"col-sm-5\">{0}{1}</div>";
        
        private const string PrefixTemplate = "<div class=\"input-group\"><div class=\"input-group-prepend\"><div class=\"input-group-text\">{1}</div></div>{0}</div>";

        private const string DateTemplate = "<div class=\"input-group\">{0}<div class=\"input-group-append\"><div class=\"input-group-text\">📅</div></div></div>";

        public static MvcHtmlString TextEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string placeholder = null, string prefix = null, string id = null, string smXClass = ColSm5)
        {
            var label = htmlHelper.LabelFor(expression);
            var textBox = htmlHelper.TextBoxFor(expression, new {placeholder, @class = $"form-control"}).ToHtmlString();
            var validationHtml = GetValidationHtml(htmlHelper, expression);
            
            if (!string.IsNullOrEmpty(prefix))
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
            htmlHelper.EnableClientValidation(false);
            var textBox = htmlHelper.TextBoxFor(expression, new { @class = "form-control datepicker " + additionalClass, type = "datetime", Value = model == null ? "" : $"{model:dd.MM.yyyy}"}).ToHtmlString();
            htmlHelper.EnableClientValidation(true);
            var validationHtml = GetValidationHtml(htmlHelper, expression);

            var textBoxHtml = string.Format(DivSmFive, string.Format(DateTemplate, textBox), validationHtml);

            return CreateFormGroup(label, textBoxHtml, id);
        }


        public static MvcHtmlString EnumRadioEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string radioColSmXClass = ColSm5, string id = null)
        {
            var label = htmlHelper.LabelFor(expression);
            var enumRadioButton = htmlHelper.EnumRadioButtonFor(expression).ToHtmlString();

            var enumRadioEditor = string.Format(DivSmX, radioColSmXClass, enumRadioButton, string.Empty);

            return CreateFormGroup(label, enumRadioEditor, id);
        }

        public static MvcHtmlString ReadonlyFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string smXClass = ColSm5)
        {
            var label = htmlHelper.LabelFor(expression);
            var display = htmlHelper.DisplayFor(expression).ToHtmlString();
            var hidden = htmlHelper.HiddenFor(expression).ToHtmlString();
            var readonlyField = string.Format(DivSmXStatic, smXClass, display, hidden);

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
            return htmlHelper.LabelFor(expression, new { @class = "col-sm-2 col-form-label" }).ToHtmlString();
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