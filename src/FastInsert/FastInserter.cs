using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;

namespace FastInsert
{

    public static class FastInserter
    {
        public static async Task<int> FastInsertAsync<T>(this IDbConnection connection, IEnumerable<T> list, 
            string tableName)
        {
            var wasClosed = connection.State == ConnectionState.Closed;

            if (wasClosed)
                connection.Open();

            var fileName = "temp.csv";
            var res = -1;

            try
            {
                var tableColumns = GetTableColumns(connection, tableName, connection.Database);

                var classFields = await WriteToCsvFileAsync(list, fileName);

                var tableDef = TableDefinitionFactory.BuildTableDefinition(classFields);

                var query = BuildQuery(tableName, tableDef, fileName);

                res = ExecuteStatementAsync(connection, query);
            }
            finally
            {
               // File.Delete(fileName);
            }

            if(wasClosed)
                connection.Close();

            return res;
        }
                

        private static int ExecuteStatementAsync(IDbConnection connection, string query)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteNonQuery();
        }
         
        private static string BuildQuery(string tableName, TableDef tableDef, string tempFilePath)
        {
            var lineEnding = Environment.NewLine;
            var fieldsExpression = FieldsExpressionBuilder.ToExpression(tableDef);

            return $@"LOAD DATA LOCAL INFILE '{tempFilePath}' 
                   INTO TABLE {tableName} 
                    COLUMNS TERMINATED BY ';' 
                    LINES TERMINATED BY '{lineEnding}'
                    IGNORE 1 LINES                    
                    {fieldsExpression}
                    ";
        }
               

        private static Task<List<CsvColumnDef>> WriteToCsvFileAsync<T>(IEnumerable<T> list, string fileName)
        {
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using TextWriter textWriter = new StreamWriter(fileStream);
            using var writer = new CsvWriter(textWriter);
            writer.Configuration.HasHeaderRecord = true;
            writer.Configuration.Delimiter = ";";

            var opt1 = writer.Configuration.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] { "O" };

            writer.Configuration.TypeConverterCache.AddConverter(typeof(Guid), new GuidConverter());

            var map = writer.Configuration.AutoMap<T>();
            
            writer.WriteRecords(list);
            return Task.FromResult(map.MemberMaps.Select(m => new CsvColumnDef
            {
                Name = m.Data.Names[0],
                MemberInfo = m.Data.Member
            }
            ).ToList());
        }
        
        private static IEnumerable<string> GetTableColumns(IDbConnection connection, string tableName, string dbName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT c.column_name
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.table_name = @tableName
                 AND c.table_schema = @schema";

            var param1 = command.CreateParameter();
            param1.ParameterName = "tableName";
            param1.Value = tableName;
            
            var param2 = command.CreateParameter();
            param2.ParameterName = "schema";
            param2.Value = dbName;

            command.Parameters.Add(param1);
            command.Parameters.Add(param2);

            using var reader = command.ExecuteReader();

            while (!reader.IsClosed && reader.Read())
            {
                var str = reader.GetString(0);
                yield return str;
            }
        }
    }
}