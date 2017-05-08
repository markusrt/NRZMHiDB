using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HaemophilusWeb.Views.Utils
{
    public static class BootstrapEditor
    {
        private const string FormGroupTemplate = "<div class=\"form-group\">{0}{1}</div>";

        private const string FormGroupTemplateWithId = "<div id=\"{2}\" class=\"form-group\">{0}{1}</div>";

        private const string ColSm5 = "col-sm-5";

        private const string DivSmX = "<div class=\"{0}\">{1}{2}</div>";

        private const string DivSmXStatic = "<div class=\"{0} form-control-static\">{1}{2}</div>";
        
        private const string DivSmFive = "<div class=\"col-sm-5\">{0}{1}</div>";
        
        private const string RequiredTemplate = "<div class=\"input-group\">{0}<span class=\"input-group-addon\"><span class=\"glyphicon glyphicon-star\"></span></span></div>";

        private const string DateTemplate = "<div class=\"input-group\">{0}<span class=\"input-group-addon\"><span class=\"glyphicon glyphicon-calendar\"></span></span></div>";

        public static MvcHtmlString TextEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string placeholder = null)
        {
            var label = htmlHelper.LabelFor(expression);
            var textBox = htmlHelper.TextBoxFor(expression, new {placeholder, @class = "form-control"}).ToHtmlString();
            var validationHtml = GetValidationHtml(htmlHelper, expression);
            
            //TODO: IsRequired does not work with fluent validations. I.e. implement FluentValidation based ModelMetadata if the view should render a required state 
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var required = modelMetadata.IsRequired;
            if (required)
            {
                textBox = string.Format(RequiredTemplate, textBox);
            }

            var textBoxHtml = string.Format(DivSmFive, textBox, validationHtml);

            return CreateFormGroup(label, textBoxHtml);
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
            Expression<Func<TModel, TProperty>> expression, string radioColSmXClass = ColSm5)
        {
            var label = htmlHelper.LabelFor(expression);
            var enumRadioButton = htmlHelper.EnumRadioButtonFor(expression).ToHtmlString();

            var enumRadioEditor = string.Format(DivSmX, radioColSmXClass, enumRadioButton, string.Empty);

            return CreateFormGroup(label, enumRadioEditor);
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
            return htmlHelper.LabelFor(expression, new { @class = "col-sm-2 control-label" }).ToHtmlString();
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