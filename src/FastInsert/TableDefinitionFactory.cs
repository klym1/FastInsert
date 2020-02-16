using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FastInsert.CsvHelper;

namespace FastInsert
{
    public static class TableDefinitionFactory
    {
        private static Dictionary<Type, Func<string, string>> _fieldOverrides = new Dictionary<Type, Func<string, string>>
        {
            [typeof(Guid)] = varName => $"UNHEX({varName})",
            [typeof(byte[])] = varName => $"UNHEX({varName})"
        };

        public static TableDef BuildTableDefinition(Type type)
        {
            var fields = TypeInfoProvider.GetClassFields(type);
            
            var columns = fields.Select(f => new ColumnDef
            {
                Name = f.Name,
                TransformFunc = _fieldOverrides.TryGetValue(MemberInfoType.GetType(f.MemberInfo), out var func) ? func : null
            }).ToList();

            return new TableDef
            {
                Columns = columns
            };
        }
    }
}
