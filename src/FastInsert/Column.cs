using System;

namespace FastInsert
{
    public class Column
    {
        public string Name { get; }
        public Func<string, string>? TransformFunc { get; }

        public Column(string name, Func<string, string>? transformFunc)
        {
            Name = name;
            TransformFunc = transformFunc;
        }
    }
}
