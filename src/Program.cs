using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DapperFastInsert
{
    class Program
    {
        static async Task Main()
        {
            var connBuilder = new MySqlConnectionStringBuilder
            {
                AllowLoadLocalInfile = true,
                Database = "fastinsert",
                UserID = "root",
                Password = "root"
            };

            var tableName = "test";

            var conn = connBuilder.ToString();

            var connection = new MySqlConnection(conn);

            var list = Enumerable.Range(1, 100000)
                .Select(it =>
                    new Table
                    {
                        Int = it,
                        Text = "text" + it,
                        DateCol = DateTime.UtcNow.AddHours(it),
                        Guid = Guid.NewGuid()
                    });
            
            var sw = Stopwatch.StartNew();
            await connection.FastInsertAsync(list, tableName);
            sw.Stop();
            Console.WriteLine($"Inserted in {sw.ElapsedMilliseconds} ms");
        }
    }

    public class Table
    {
        public Guid Guid { get; set; }
        public DateTime DateCol { get; set; }
        public int Int { get; set; }
        public string Text { get; set; }
    }
}
