using System.Data;
using System.Threading.Tasks;

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

        public static Task<int> ExecuteAsync(this IDbConnection connection, string query)
        {
            return Task.Run(() =>
            {
                using var command = connection.CreateCommand();
                command.CommandText = query;
                return command.ExecuteNonQuery();
            });
        }
    }
}