using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration.Internal;
using ServiceStack;

namespace HaemophilusWeb.Utils
{
    /// <summary>
    ///     Contains utility and extension methods to deal with enums.
    /// </summary>
    public static class EnumUtils
    {
        public static TEnum ParseCommaSeperatedListOfNamesAsFlagsEnum<TEnum>(string commaSeperatedList)
            where TEnum : struct, IConvertible
        {
            commaSeperatedList = commaSeperatedList ?? string.Empty;
            var enabledFlagNames = commaSeperatedList.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            return (TEnum) (object)
                enabledFlagNames.Aggregate(0, (acc, v) => acc | (int) Enum.Parse(typeof (TEnum), v), acc => acc);
        }

        public static List<TEnum> ParseCommaSeperatedListOfNames<TEnum>(string commaSeperatedList)
            where TEnum : struct, IConvertible
        {
            commaSeperatedList = commaSeperatedList ?? string.Empty;
            var enumEntries = commaSeperatedList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return enumEntries.Where(v => !string.IsNullOrEmpty(v.Trim())).Select(e => (TEnum)Enum.Parse(typeof(TEnum), e)).ToList();
        }

        /// <summary>
        ///     Creates a list of all available values for a given <typeparamref name="TEnum" />
        /// </summary>
        /// <typeparam name="TEnum">Type of Enum to get values of</typeparam>
        /// <returns>
        ///     List with enum values, never <c>null</c>
        /// </returns>
        public static IEnumerable<TEnum> AllEnumValues<TEnum>()
        {
            return Enum.GetValues(GetEnumType<TEnum>()).Cast<TEnum>().ToList();
        }

        /// <summary>
        ///     Creates a list of all available values for a given <typeparamref name="TEnum" /> formatted as string
        /// </summary>
        /// <returns>
        ///     List with enum values, never <c>null</c>
        /// </returns>
        public static string AllEnumValuesToString<TEnum>() where TEnum : struct, IConvertible
        {
            var allEnumValues = AllEnumValues<TEnum>();
            var enumStrings =
                allEnumValues.Select(e => string.Format("{0} ({1})", e, e.ToInt32(CultureInfo.InvariantCulture)));

            return string.Join(", ", enumStrings);
        }

        /// <summary>
        ///     Searches the first attribute of type  <typeparamref name="TAttribute" /> of a given enum value
        /// </summary>
        /// <typeparam name="TAttribute"> Attribute type to search for </typeparam>
        /// <param name="enumValue"> Enum value to search for attribute </param>
        /// <returns>
        ///     First attribute or <c>null</c> if no such attribute exists
        /// </returns>
        public static TAttribute FirstAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            TAttribute result = null;
            var type = enumValue.GetType();
            var filterAttributeName = enumValue.ToString();

            var memInfo = type.GetMember(filterAttributeName);
            if (memInfo.Any())
            {
                var attributes = memInfo[0].GetCustomAttributes(
                    typeof (TAttribute), false);
                if (attributes.Any())
                {
                    result = (TAttribute) attributes[0];
                }
            }
            return result;
        }

        /// <summary>
        ///     Checks if an enum contains only valid (i.e. defined) values. Flag enums are supported as well.
        /// </summary>
        /// <typeparam name="TEnum">Type of Enum to check value for</typeparam>
        /// <param name="value">Enum value containing</param>
        /// <returns>
        ///     true if value contains a defined value or a combination of valid enum bits (for flags), otherwise false.
        /// </returns>
        public static bool IsDefinedEnumValue<TEnum>(this TEnum value) where TEnum : struct, IConvertible
        {
            var isFlagsEnum = IsFlagsEnum<TEnum>();
            return isFlagsEnum
                ? value.IsDefinedFlag()
                : value.IsDefinedExplicitly();
        }

        private static bool IsDefinedExplicitly<TEnum>(this TEnum value) where TEnum : struct, IConvertible
        {
            return
                new List<string>(Enum.GetNames((typeof (TEnum)))).Contains(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///     Checks if a given enum type has the <see cref="FlagsAttribute" /> attached
        /// </summary>
        /// <returns>true for flag enums, otherwise false</returns>
        public static bool IsFlagsEnum<TEnum>()
        {
            return GetEnumType<TEnum>().GetCustomAttribute<FlagsAttribute>() != null;
        }

        /// <summary>
        ///     Checks if a enum flags value contains only valid enum bits
        /// </summary>
        /// <typeparam name="TEnum">Type of Enum to check all valid bits</typeparam>
        /// <param name="value">Enum value containing combined (OR) enum bit values</param>
        /// <returns>
        ///     true if value contains only a combination of valid enum bits, otherwise false.
        /// </returns>
        private static bool IsDefinedFlag<TEnum>(this TEnum value) where TEnum : struct, IConvertible
        {
            return ClearAllDefinedFlags(value) == 0;
        }

        /// <summary>
        ///     Clears all defined bit flags of <typeparamref name="TEnum" /> from a given enum value
        /// </summary>
        /// <param name="value">Enum value to clear all flags from</param>
        /// <returns>The remaining integer value after all defined flags have been cleared</returns>
        private static int ClearAllDefinedFlags<TEnum>(TEnum value) where TEnum : struct, IConvertible
        {
            var allDefinedBits = AllEnumValues<TEnum>().Cast<int>().ToArray();
            return ClearBits(value, allDefinedBits);
        }

        private static int ClearBits<TEnum>(TEnum value, params int[] bits) where TEnum : struct, IConvertible
        {
            var remainder = value.ToInt32(CultureInfo.InvariantCulture);
            remainder = bits.Aggregate(remainder, (current, enumerationValue) => current & ~enumerationValue);
            return remainder;
        }

        /// <summary>
        ///     Determines whether any of the given bit fields are set in the enum instance.
        /// </summary>
        /// <param name="thisEnum">Enum value to check for bit fields</param>
        /// <param name="flags">A list of enumeration values</param>
        /// <returns>true if any of the bit fields of flags is also set in the enum instance; otherwise, false.</returns>
        public static bool HasAnyFlag(this Enum thisEnum, params Enum[] flags)
        {
            return flags.Any(thisEnum.HasFlag);
        }

        public static string GetEnumDescription<TEnum>(object enumValue)
        {
            var value = enumValue.ToString();
            var enumType = GetEnumType<TEnum>();
            if (enumType == typeof (object))
            {
                enumType = GetEnumType(enumValue.GetType());
            }
            if (enumValue is int)
            {
                value = Enum.GetName(enumType, enumValue);
            }
            if (value == null)
            {
                return enumValue.ToString();
            }
            var field = (enumType.IsNullableType() ? PrimitiveHelper.GetTypeOfNullable(enumType) : enumType).GetField(value);
            if (field == null)
            {
                return value;
            }
            var attributes =
                (DescriptionAttribute[]) field.GetCustomAttributes(typeof (DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value;
        }

        private static Type GetEnumType<TEnum>()
        {
            var enumType = typeof (TEnum);
            return GetEnumType(enumType);
        }

        private static Type GetEnumType(Type enumType)
        {
            if (enumType.IsNullableType())
            {
                enumType = Nullable.GetUnderlyingType(enumType);
            }
            return enumType;
        }
    }
}