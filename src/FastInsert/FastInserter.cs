using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using FastInsert.CsvHelper;

namespace FastInsert
{
    public static class FastInserter
    {
        public static async Task FastInsertAsync<T>(this IDbConnection connection,
            IEnumerable<T> list,
            Action<FastInsertConfig>? conf = null)
        {
            EnsureMySqlConnection(connection);

            var config = GetConfig<T>(conf);

            if (!ConnectionStringValidator.ConnectionStringValid(connection.ConnectionString, out var error))
                throw new ArgumentException(error);

            var tableName = config.TableNameResolver.GetTableName();

            var writer = CsvWriterConfigurator.GetWriter<T>();
            
            var tableDef = TableDefinitionFactory.BuildTableDefinition<T>();

            foreach (var partition in EnumerableExtensions.GetPartitions(list, config.BatchSize))
            {
                var fileName = $"{Guid.NewGuid()}.csv";

                try
                {
                    var csvSettings = new CsvFileSettings
                    {
                        Delimiter = ";;",
                        LineEnding = Environment.NewLine,
                        Path = fileName,
                        FieldEscapedByChar = "\\\\",
                        FieldEnclosedByChar = "",
                    };

                    var query = BuildLoadDataQuery.BuildQuery(tableName, tableDef, csvSettings);

                    await writer.WriteAsync(partition, csvSettings);
                    await connection.ExecuteAsync(query);
                }
                finally
                {
                    config.Writer?.WriteLine(fileName + ":");
                    config.Writer?.WriteLine(File.ReadAllText(fileName));

                    File.Delete(fileName);
                }
            }
        }

        private static void EnsureMySqlConnection(IDbConnection connection)
        {
            var type = connection.GetType().ToString();

            if (!type.Contains("MySqlConnection"))
                throw new ArgumentException("This extension can only be used with MySqlConnection");
        }

        private static FastInsertConfig GetConfig<T>(Action<FastInsertConfig> conf)
        {
            var config = new FastInsertConfig(typeof(T));
            conf?.Invoke(config);
            return config;
        }
    }
}
