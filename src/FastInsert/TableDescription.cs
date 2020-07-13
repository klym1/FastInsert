using System;

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
}
