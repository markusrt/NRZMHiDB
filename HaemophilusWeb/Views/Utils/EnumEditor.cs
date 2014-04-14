using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HaemophilusWeb.Views.Utils
{
    public static class EnumEditor
    {
        private static readonly SelectListItem[] SingleEmptyItem = new[] {new SelectListItem {Text = "", Value = ""}};

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
            var field = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[]) field.GetCustomAttributes(typeof (DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression, object htmlAttributes = null)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            
            var items = from value in GetEnumValues<TEnum>(modelMetadata)
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
            Expression<Func<TModel, TEnum>> expression, bool shortName=false)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var currentValue = htmlHelper.ValueFor(expression).ToString();

            var sb = new StringBuilder();
            sb.Append("<div><div class=\"btn-group\" data-toggle=\"buttons\">");
            foreach (var value in GetEnumValues<TEnum>(modelMetadata))
            {
                var description = GetEnumDescription(value);
                if (shortName)
                {
                    description = description.First().ToString();
                }
                var name = GetName(modelMetadata, value);

                var id = string.Format("{0}_{1}", modelMetadata.PropertyName, name);

                var radio = htmlHelper.RadioButtonFor(expression, name, new {id}).ToHtmlString();
                sb.AppendFormat("<label class=\"btn btn-default {2}\">{0} {1}</label>", radio, description, currentValue==name?"active":"");
            }
            sb.Append("</div></div>");
            sb.Append(htmlHelper.ValidationMessageFor(expression).ToHtmlString());
            return MvcHtmlString.Create(sb.ToString());
        }

        private static string GetName<TEnum>(ModelMetadata metadata, TEnum value)
        {
            var enumType = GetNonNullableModelType(metadata);
            return Enum.GetName(enumType, value);
        }
    }
}