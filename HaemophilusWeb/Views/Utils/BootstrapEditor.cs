using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HaemophilusWeb.Views.Utils
{
    public static class BootstrapEditor
    {
        private const string FormGroupTemplate = "<div class=\"form-group\">{0}{1}</div>";

        private const string TextBoxTemplate = "<div class=\"col-sm-5\">{0}{1}</div>";
        
        private const string RequiredTemplate = "<div class=\"input-group\">{0}<span class=\"input-group-addon\"><span class=\"glyphicon glyphicon-star\"></span></span></div>";

        public static MvcHtmlString TextEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string placeholder = null)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var required = modelMetadata.IsRequired;
            var label = htmlHelper.LabelFor(expression, new {@class = "col-sm-2 control-label"}).ToHtmlString();
            var textBox = htmlHelper.TextBoxFor(expression, new {placeholder, @class = "form-control"}).ToHtmlString();
            var validationHtml = GetValidationHtml(htmlHelper, expression);

            if (required)
            {
                textBox = string.Format(RequiredTemplate, textBox);
            }

            var textBoxHtml = string.Format(TextBoxTemplate, textBox, validationHtml);

            return MvcHtmlString.Create(string.Format(FormGroupTemplate, label, textBoxHtml));
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