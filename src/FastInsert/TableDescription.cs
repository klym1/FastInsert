using System;
using System.Collections.Generic;
using System.Reflection;

namespace FastInsert
{
    public class ColumnDef
    {
        public string Name { get; }
        public Func<string, string>? TransformFunc { get; }

        public ColumnDef(string name, Func<string, string>? transformFunc)
        {
            Name = name;
            TransformFunc = transformFunc;
        }
    }

    public class CsvColumnDef
    {
        public string Name { get; }
        public MemberInfo MemberInfo { get; }

        public CsvColumnDef(string name, MemberInfo memberInfo)
        {
            Name = name;
            MemberInfo = memberInfo;
        }
    }
}
