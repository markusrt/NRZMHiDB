using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Views.Utils
{
    public static class EnumEditor
    {
        public const string HiddenOnUserInterface = "~~hidden~~";
        private static readonly SelectListItem[] SingleEmptyItem = {new SelectListItem {Text = "", Value = ""}};

        private static Type GetNonNullableModelType(ModelMetadata modelMetadata)
        {
            var realModelType = modelMetadata.ModelType;

            var underlyingType = Nullable.GetUnderlyingType(realModelType);
            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }
            return realModelType;
        }

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (!EnumUtils.IsFlagsEnum<TEnum>())
            {
                return EnumUtils.GetEnumDescription<TEnum>(value);
            }

            var integerValue = (int) (object) value;
            var descriptions = EnumUtils.AllEnumValues<TEnum>().Cast<int>()
                .Where(enumValue => enumValue != 0 && (enumValue & integerValue) == enumValue)
                .Select(enumValue => EnumUtils.GetEnumDescription<TEnum>(enumValue)).ToList();
            return string.Join(", ", descriptions);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression, object htmlAttributes = null) where TEnum : struct, IConvertible
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            var items = from value in GetEnumValues<TEnum>(modelMetadata)
                orderby GetEnumDescription(value)
                where GetEnumDescription(value) != HiddenOnUserInterface
                select new SelectListItem
                {
                    Text = GetEnumDescription(value),
                    Value = value.ToString(),
                    Selected = value.Equals(modelMetadata.Model)
                };

            // If the enum is nullable, add an 'empty' item to the collection
            if (modelMetadata.IsNullableValueType)
            {
                items = SingleEmptyItem.Concat(items);
            }

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }

        private static IEnumerable<TEnum> GetEnumValues<TEnum>(ModelMetadata modelMetadata)
        {
            var enumType = GetNonNullableModelType(modelMetadata);
            var values = Enum.GetValues(enumType).Cast<TEnum>();
            return values;
        }


        public static MvcHtmlString EnumRadioButtonFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression, bool shortName = false, string suffix = null)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var currentValue = htmlHelper.ValueFor(expression).ToString();
            var enumType = GetNonNullableModelType(modelMetadata);
            var currentEnumValue = currentValue == string.Empty ? default : Enum.Parse(enumType, currentValue);
            var isFlagsEnum = enumType.GetCustomAttributes<FlagsAttribute>().Any();

            var sb = new StringBuilder();
            // Bootstrap 5 .btn-check pattern: a visually hidden input followed by a styled label. This
            // replaces Bootstrap 3's data-toggle="buttons" plugin, which was removed in Bootstrap 5.
            sb.Append("<div><div class=\"btn-group flex-wrap\" role=\"group\">");
            foreach (var value in GetEnumValues<TEnum>(modelMetadata))
            {
                var intValue = Convert.ToInt32(value);
                var description = GetEnumDescription(value);

                if (isFlagsEnum && intValue == 0 || description == HiddenOnUserInterface)
                {
                    continue;
                }

                var name = GetName(modelMetadata, value);
                var id = $"{modelMetadata.PropertyName}_{name}";
                var isActive = currentValue == name || isFlagsEnum && (intValue & Convert.ToInt32(currentEnumValue)) == intValue;
                if (shortName)
                {
                    description = description.First().ToString();
                }

                var selectHtml = isFlagsEnum ?
                    $"<input type=\"checkbox\" class=\"btn-check\" id=\"{id}\" name=\"{modelMetadata.PropertyName}\" value=\"{value}\" autocomplete=\"off\" {(isActive ? "checked" : string.Empty)}/>"
                    : htmlHelper.RadioButtonFor(expression, name, new { id, @class = "btn-check", autocomplete = "off" }).ToHtmlString();

                sb.AppendFormat("{0}<label class=\"btn btn-outline-secondary\" for=\"{1}\">{2}</label>", selectHtml, id, description);
            }
            sb.Append("</div>");
            if (suffix != null)
            {
                sb.Append($"<span style=\"margin-left:0.5em\" class=\"badge text-bg-light\">{suffix}</span>");
            }
            sb.Append("</div>");
            sb.Append(htmlHelper.ValidationMessageFor(expression)?.ToHtmlString());
            return MvcHtmlString.Create(sb.ToString());
        }

        private static string GetName<TEnum>(ModelMetadata metadata, TEnum value)
        {
            var enumType = GetNonNullableModelType(metadata);
            return Enum.GetName(enumType, value);
        }
    }
}