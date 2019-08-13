using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastInsert
{
    public static class TableDefinitionFactory
    {
        private static Dictionary<Type, Func<string, string>> _fieldOverrides = new Dictionary<Type, Func<string, string>>
        {
            [typeof(Guid)] = varName => $"UNHEX({varName})"
        };

        public static TableDef BuildTableDefinition(IEnumerable<CsvColumnDef> fields)
        {
            var columns = fields.Select(f => new ColumnDef
            {
                Name = f.Name,
                RequiresTransformation = _fieldOverrides.ContainsKey((f.MemberInfo as PropertyInfo).PropertyType),
                TransformFunc = _fieldOverrides.TryGetValue((f.MemberInfo as PropertyInfo).PropertyType, out var func) ? func : (f => f)
            }).ToList();

            return new TableDef
            {
                Columns = columns
            };
        }
    }
}