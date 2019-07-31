using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using MySql.Data.MySqlClient;


namespace DapperFastInsert
{
    class Program
    {
        static async Task Main()
        {
            var connBuilder = new MySqlConnectionStringBuilder()
            {
                AllowLoadLocalInfile = true,
                Database = "fastinsert",
                UserID = "root",
                Password = "root"
            };

            var conn = connBuilder.ToString();

            var connection = new MySqlConnection(conn);

            var list = Enumerable.Range(1, 10)
                .Select(it =>
                    new Table
                    {
                        Int = it,
                        Text = "text" + it
                    });

            var fileName = "temp.csv";

            var tableColumns = GetTableColumns(connection, "test");

            var columnIndexes = GetColumnIndexes(tableColumns);
            await WriteToCsvFileAsync(list, columnIndexes, fileName);

            var query = BuildQuery("test", fileName);

            var sw = Stopwatch.StartNew();
            await connection.ExecuteAsync(query);
            sw.Stop();
            Console.WriteLine($"Inserted in {sw.ElapsedMilliseconds} ms");
        }

        private static IDictionary<string, int> GetColumnIndexes(IEnumerable<string> columns)
        {
            return columns
                .Select((it, index) => (it, index))
                .ToDictionary(it => it.it, it => it.index, StringComparer.OrdinalIgnoreCase);
        }

        private static string BuildQuery(string tableName, string tempFilePath)
        {
            var lineEnding = Environment.NewLine;

            return $@"LOAD DATA LOCAL INFILE '{tempFilePath}' 
                   INTO TABLE `{tableName}` 
                   FIELDS TERMINATED BY ';' 
                   LINES TERMINATED BY '{lineEnding}' 
                   IGNORE 1 LINES";

        }

        private static Task WriteToCsvFileAsync(IEnumerable<Table> list, IDictionary<string, int> dict, string fileName)
        {
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using TextWriter textWriter = new StreamWriter(fileStream);
            using var writer = new CsvWriter(textWriter);

            writer.Configuration.HasHeaderRecord = true;
            writer.Configuration.DynamicPropertySort = new ColumnComparer();

            var map = writer.Configuration.AutoMap<Table>();

            foreach (var memberMap in map.MemberMaps)
            {
                var index = dict[memberMap.Data.Names[0]];
                memberMap.Index(index);
            }
            
            writer.WriteRecords(list);
            return Task.FromResult(0);
        }

        private static IEnumerable<string> GetTableColumns(IDbConnection connection, string tableName)
        {
            return connection.Query<string>($@"SELECT c.column_name
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.table_name = '{tableName}'
                -- AND c.table_schema = 'db_name'");
        }
    }

    public class ColumnComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.Compare(x, y, StringComparison.InvariantCulture) + 1;
        }
    }

    public class Table
    {
        public int Int { get; set; }
        public string Text { get; set; }
    }
}
