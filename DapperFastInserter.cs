using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Dapper;

namespace DapperFastInsert
{
    public static class DapperFastInserter
    {
        public static async Task<int> FastInsertAsync<T>(this IDbConnection connection, IEnumerable<T> list, string tableName)
        {
            var fileName = "temp.csv";
            
            var tableColumns = GetTableColumns(connection, tableName, connection.Database);
            var columnIndexes = GetColumnIndexes(tableColumns);
            await WriteToCsvFileAsync(list, columnIndexes, fileName);

            var query = BuildQuery(tableName, fileName);
            return await connection.ExecuteAsync(query);
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

        private static Task WriteToCsvFileAsync<T>(IEnumerable<T> list, IDictionary<string, int> dict, string fileName)
        {
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using TextWriter textWriter = new StreamWriter(fileStream);
            using var writer = new CsvWriter(textWriter);
            writer.Configuration.HasHeaderRecord = true;

            var opt1 = writer.Configuration.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] { "O" };

            //var opt2 = writer.Configuration.TypeConverterOptionsCache.GetOptions<Guid>();
            //opt2.Formats = new[] { "N" };
            
            writer.Configuration.TypeConverterCache.AddConverter(typeof(Guid), new GuidFormatter());

            var map = writer.Configuration.AutoMap<T>();
            SortColumns(dict, map);

            writer.WriteRecords(list);
            return Task.FromResult(0);
        }

        private static void SortColumns(IDictionary<string, int> dict, ClassMap map)
        {
            foreach (var memberMap in map.MemberMaps)
            {
                var index = dict[memberMap.Data.Names[0]];
                memberMap.Index(index);
            }
        }

        private static IEnumerable<string> GetTableColumns(IDbConnection connection, string tableName, string dbName)
        {
            return connection.Query<string>($@"SELECT c.column_name
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.table_name = '{tableName}'
                -- AND c.table_schema = '{dbName}'");
        }
    }

    public class GuidFormatter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var bytes = ((Guid)value).ToByteArray();
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }
    }
}