using System;

namespace FastInsert
{
    public static class TypeExtensions
    {
        public static bool IsNullableEnum(this Type it)
        {
            return IsNullable(it, out var underlying) && (underlying?.IsEnum ?? false);
        }

        public static bool IsNullable(this Type t, out Type underlying)
        {
            underlying = Nullable.GetUnderlyingType(t);
            return underlying != null;
        } 
    }
}
