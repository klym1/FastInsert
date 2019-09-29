using System;

namespace FastInsert
{
    internal static class ConnectionStringValidator
    {
        public static bool ConnectionStringValid(string connString, out string o)
        {
            var connStr = ConnectionStringParser.Parse(connString);

            o = "";

            if (!connStr.AllowUserVariables)
                o = "AllowUserVariables variable must be set to 'true' in order to perform data transformations";

            if (!connStr.AllowLoadLocalInfile)
                o = "AllowLoadLocalInfile variable must be set to 'true' in order to allow MySql Load infile operation";

            return string.IsNullOrEmpty(o);
        }
    }
}