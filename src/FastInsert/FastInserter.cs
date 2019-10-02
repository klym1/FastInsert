using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace FastInsert
{
    public static class FastInserter
    {
        public static async Task FastInsertAsync<T>(this IDbConnection connection,
            IEnumerable<T> list,
            Action<FastInsertConfig> conf = null)
        {
            var config = new FastInsertConfig(typeof(T));

            conf?.Invoke(config);

            var type = connection.GetType().ToString();

            if (!type.Contains("MySqlConnection"))
                throw new ArgumentException("This extension can only be used with MySqlConnection");

            if (!ConnectionStringValidator.ConnectionStringValid(connection.ConnectionString, out var error))
                throw new ArgumentException(error);

            var wasClosed = connection.State == ConnectionState.Closed;

            if (wasClosed)
                connection.Open();

            var tableName = config.TableNameResolver.GetTableName();

            var tableColumns = GetTableColumns(connection, tableName, connection.Database);
            var classConfig = GetConfiguration<T>();
            var classFields = GetClassFields(classConfig);
            var tableDef = TableDefinitionFactory.BuildTableDefinition(classFields);
            
            foreach (var partition in EnumerableExtensions.GetPartitions(list, config.BatchSize))
            {
                lock (connection)
                {
                    var fileName = $"{Guid.NewGuid()}.csv";

                    try
                    {
                        var query = BuildQuery(tableName, tableDef, fileName);

                        WriteToCsvFileAsync(classConfig, partition, fileName);
                        connection.Execute(query);
                    }
                    finally
                    {
                        config.Writer?.WriteLine(fileName + ":");
                        config.Writer?.WriteLine(File.ReadAllText(fileName));

                        File.Delete(fileName);
                    }
                }
            }
            
            if (wasClosed)
                connection.Close();
        }

        private static IEnumerable<CsvColumnDef> GetClassFields(ClassMap map)
        {
            return map.MemberMaps.Select(m => new CsvColumnDef
                {
                    Name = m.Data.Names[0],
                    MemberInfo = m.Data.Member
                }
            );
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

        private static ClassMap<T> GetConfiguration<T>()
        {
            var conf = new Configuration
            {
                HasHeaderRecord = true, 
                Delimiter = ";"
            };

            var opt1 = conf.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] { "O" };

            conf.TypeConverterCache.AddConverter(typeof(Guid), new GuidConverter());

            var map = conf.AutoMap<T>();
            return map;
        }

        private static void WriteToCsvFileAsync<T>(ClassMap<T> classMap, IEnumerable<T> list, string fileName)
        {
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using var textWriter = new StreamWriter(fileStream);
            using var writer = new CsvWriter(textWriter);
            writer.Configuration.RegisterClassMap(classMap);
            writer.WriteRecords(list);
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