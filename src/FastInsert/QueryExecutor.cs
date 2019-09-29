using System.Data;

namespace FastInsert
{
    internal static class QueryExecutor
    {
        public static int Execute(this IDbConnection connection, string query)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            return command.ExecuteNonQuery();
        }
    }
}