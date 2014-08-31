using System;
using System.Linq;

namespace HaemophilusWeb.Utils
{
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
    }
}