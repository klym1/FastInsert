using System;
using System.Reflection;

namespace FastInsert
{
    public static class MemberInfoType
    {
        public static Type GetType(this MemberInfo info)
        {
            return info switch
            {
                PropertyInfo p => p.PropertyType,
                FieldInfo f => f.FieldType,
                _ => throw new NotSupportedException()
            };
        }
        
    }
}
