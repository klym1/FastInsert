using System;
using System.Collections.Generic;
using System.Linq;

namespace FastInsert.CsvHelper
{
    public static class TypeInfoProvider
    {
        private static Func<string, string>? GetTransformer(Type propType, BinaryFormat format)
        {
            if (!IsBinary(propType)) 
                return null;

            return format switch
            {
                BinaryFormat.Base64 => Transformation.FromBase64,
                BinaryFormat.Hex => Transformation.FromHex,
                _ => null
            };
        }

        private static bool IsBinary(Type t)
        {
            var actualType = t.IsNullable(out var underlying) ? underlying : t;
            return actualType == typeof(byte[]) || actualType == typeof(Guid);
        }

        public static IEnumerable<Column> GetClassFields(Type type, BinaryFormat format) =>
            ClassAutoMapper.AutoMap(type)
                .MemberMaps
                .Where(m => !m.Data.Ignore)
                .Select(m => new Column
                    (
                        m.Data.Names[0],
                        GetTransformer(MemberInfoType.GetType(m.Data.Member), format)
                    )
                );
    }
}
