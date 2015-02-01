using System;
using System.ComponentModel;
using System.Linq;

namespace HaemophilusWeb.Utils
{
    public static class EnumSerializer
    {
        private const string InvalidFlagsEnumValueErrorMessage =
            "'{0}' is not a valid value for '{1}'; valid values are flag combinations built via bitwise OR on the following values: {2}";

        private const string InvalidEnumValueErrorMessage =
            "'{0}' is not a valid value for '{1}'; valid values are '{2}'";

        private const string InvalidDefaultValueErrorMessage = "'{0}' is not a default value for '{1}'";

        /// <summary>
        ///     Deserializes a string to a corresponding enum value. Input string is parsed case insensitive.
        /// </summary>
        /// <typeparam name="TEnum">Target enum type</typeparam>
        /// <exception cref="ArgumentException">If deserialization fails, i.e. null string, or invalid value</exception>
        /// <param name="value">
        ///     String value to be deserialized, must not be <c>null</c> or empty
        /// </param>
        /// <returns>
        ///     An object of type <typeparamref name="TEnum" /> whose value is represented by value.
        /// </returns>
        public static TEnum DeserializeEnumStrict<TEnum>(string value) where TEnum : struct, IConvertible
        {
            return TryParseDefinedOnly(value, default(TEnum), true);
        }

        /// <summary>
        ///     Executes <see cref="Enum.TryParse{TEnum}(string,out TEnum)" /> and validates
        ///     if the result is a defined enum value.
        /// </summary>
        /// <typeparam name="TEnum">Target enum type</typeparam>
        /// <param name="value">
        ///     String value to be deserialized, may be <c>null</c> or empty
        /// </param>
        /// <param name="defaultValue">If strict is set to false, Value to return as default if parsing fails.</param>
        /// <param name="strict">
        ///     Method throws an <see cref="ArgumentException" /> if set to true, defaults to false.
        /// </param>
        /// <returns>
        ///     An object of type <typeparamref name="TEnum" /> whose value is represented by value.
        /// </returns>
        private static TEnum TryParseDefinedOnly<TEnum>(string value, TEnum defaultValue = default(TEnum),
            bool strict = false)
            where TEnum : struct, IConvertible
        {
            ThrowInternalErrorIfDefaultValueIsInvalid(defaultValue);

            TEnum result;
            var parseSuccess = Enum.TryParse(value, true, out result);
            var invalidEnumValue = (!parseSuccess || !result.IsDefinedEnumValue());

            if (invalidEnumValue && !string.IsNullOrEmpty(value))
            {
                var enumType = typeof (TEnum);
                foreach (var enumValue in Enum.GetValues(enumType))
                {
                    var enumName = Enum.GetName(enumType, enumValue);
                    var memInfo = enumType.GetMember(enumName);
                    var attributes = memInfo[0].GetCustomAttributes(typeof (DescriptionAttribute), false);
                    if (attributes.Any())
                    {
                        var name = ((DescriptionAttribute) attributes[0]).Description;
                        if (value.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            result = (TEnum) enumValue;
                            invalidEnumValue = false;
                        }
                        if (true) //CheckIfAllEnumValuesStartWithADifferentLetter<TEnum>(enumType))
                        {
                            if (name.ToLower().StartsWith(value.ToLower()))
                            {
                                result = (TEnum) enumValue;
                                invalidEnumValue = false;
                            }
                        }
                    }
                }
            }

            if (invalidEnumValue && string.IsNullOrEmpty(value))
            {
                result = default(TEnum);
                invalidEnumValue = false;
            }

            if (strict && invalidEnumValue)
            {
                ThrowBadRequestOnInvalidEnumValue<TEnum>(value);
            }
            if (invalidEnumValue)
            {
                result = defaultValue;
            }
            return result;
        }

        private static bool CheckIfAllEnumValuesStartWithADifferentLetter<TEnum>(Type enumType)
            where TEnum : struct, IConvertible
        {
            return Enum.GetNames(enumType).Select(n => n.First()).Distinct().Count() == Enum.GetValues(enumType).Length;
        }

        private static void ThrowInternalErrorIfDefaultValueIsInvalid<TEnum>(TEnum defaultValue)
            where TEnum : struct, IConvertible
        {
            if (defaultValue.IsDefinedEnumValue())
            {
                return;
            }
            throw new Exception(string.Format(InvalidDefaultValueErrorMessage, defaultValue, typeof (TEnum).Name));
        }

        private static void ThrowBadRequestOnInvalidEnumValue<TEnum>(string value) where TEnum : struct, IConvertible
        {
            var enumTypeName = typeof (TEnum).Name;
            var allEnumValues = EnumUtils.AllEnumValuesToString<TEnum>();

            throw new Exception(string.Format(InvalidEnumValueErrorMessage, value, enumTypeName, allEnumValues));
        }
    }
}