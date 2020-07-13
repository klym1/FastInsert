using System;
using System.Linq;
using FastInsert.CsvHelper;

namespace FastInsert
{
    public static class TableDefinitionFactory
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

        private static bool IsBinary(Type t) => t == typeof(byte[]) || t == typeof(Guid);
        
        public static TableDef BuildTableDefinition(Type type, BinaryFormat format)
        {
            var fields = TypeInfoProvider.GetClassFields(type);
            
            var columns = fields.Select(f => new ColumnDef
            {
                Name = f.Name,
                TransformFunc = GetTransformer(MemberInfoType.GetType(f.MemberInfo), format)
            }).ToList();

            return new TableDef
            {
                Columns = columns
            };
        }
    }
}
