using System;
using System.Collections.Generic;
using System.Reflection;

namespace FastInsert
{
    public class TableDef
    {
        public List<ColumnDef> Columns { get; set; }
    }

    public class ColumnDef
    {
        public string Name { get; set; }
        public bool RequiresTransformation { get; set; }
        public Func<string, string> TransformFunc { get; set; }
    }

    public class CsvColumnDef
    {
        public string Name { get; set; }
        public MemberInfo MemberInfo { get; set; }
    }
}