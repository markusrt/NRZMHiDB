using System;

namespace HaemophilusWeb.Utils
{
    public static class TypeUtils
    {
        public static Type GetTypeOrNullableType(this object o)
        {
            var type = o.GetType();
            var nullableType = Nullable.GetUnderlyingType(type);

            return nullableType ?? type;
        }

        public static Type GetTypeOrNullableType(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);

            return nullableType ?? type;
        }
    }
}