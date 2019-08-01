using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FastInsert;
using MySql.Data.MySqlClient;

namespace FastInsertExample.Console
{
    class Program
    {
        static async Task Main()
        {
            var connBuilder = new MySqlConnectionStringBuilder
            {
                AllowLoadLocalInfile = true,
                AllowUserVariables = true,
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
                    }).ToList();
            
            var sw = Stopwatch.StartNew();

            await connection.FastInsertAsync(list, tableName);
            sw.Stop();
            System.Console.WriteLine($"Inserted in {sw.ElapsedMilliseconds} ms");
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
