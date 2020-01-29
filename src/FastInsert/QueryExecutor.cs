using System.Data;
using System.Threading.Tasks;

namespace FastInsert
{
    internal static class QueryExecutor
    {
        public static Task<int> ExecuteAsync(this IDbConnection connection, string query)
        {
            return Task.Run(() =>
            {
                var wasClosed = connection.State == ConnectionState.Closed;

                if (wasClosed)
                    connection.Open();
                
                try
                {

                    using var command = connection.CreateCommand();
                    command.CommandText = query;
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    if (wasClosed)
                        connection.Close();
                }
            });
        }
    }
}
