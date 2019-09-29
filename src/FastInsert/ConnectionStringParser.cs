using System;
using System.Collections.Generic;
using System.Linq;

namespace FastInsert
{
    public class ConnectionString
    {
        public bool AllowLoadLocalInfile { get; set; }
        public bool AllowUserVariables { get; set; }
    }

    public class ConnectionStringParser
    {
        public static ConnectionString Parse(string connString)
        {
            var pairs = connString
                .Split(';')
                .Select(it => it.Split('='))
                .Select(it => (Key: it[0], Value: it[1]))
                .ToDictionary(it => it.Key, it => it.Value, StringComparer.OrdinalIgnoreCase);

            return new ConnectionString
            {
                AllowLoadLocalInfile = HasAndTrue(pairs, "AllowLoadLocalInfile"),
                AllowUserVariables = HasAndTrue(pairs, "AllowUserVariables")
            };
        }

        private static bool HasAndTrue(IReadOnlyDictionary<string, string> pairs, string varName)
        {
            return pairs.TryGetValue(varName, out var val) && IsTrue(val);
        }

        private static readonly string[] TruthyValues = {"true", "1"};

        private static bool IsTrue(string val)
        {
            return !string.IsNullOrEmpty(val)
                   && TruthyValues.Contains(val, StringComparer.OrdinalIgnoreCase);
        }
    }
}