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
            Action<FastInsertConfig>? conf = null)
        {
            EnsureMySqlConnection(connection);

            var config = GetConfig<T>(conf);

            if (!ConnectionStringValidator.ConnectionStringValid(connection.ConnectionString, out var error))
                throw new ArgumentException(error);

            var wasClosed = connection.State == ConnectionState.Closed;

            if (wasClosed)
                connection.Open();

            var tableName = config.TableNameResolver.GetTableName();

            var tableColumns = DbHelpers.GetTableColumns(connection, tableName, connection.Database);
            var classConfig = GetConfiguration<T>();
            var classFields = GetClassFields(classConfig);
            var tableDef = TableDefinitionFactory.BuildTableDefinition(classFields);

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

                    CsvFileWriter.WriteToCsvFileAsync(classConfig, partition, csvSettings);
                    await connection.ExecuteAsync(query);
                }
                finally
                {
                    config.Writer?.WriteLine(fileName + ":");
                    config.Writer?.WriteLine(File.ReadAllText(fileName));

                    File.Delete(fileName);
                }
            }

            if (wasClosed)
                connection.Close();
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

        private static IEnumerable<CsvColumnDef> GetClassFields(ClassMap map)
        {
            return map.MemberMaps.Select(m => new CsvColumnDef
                {
                    Name = m.Data.Names[0],
                    MemberInfo = m.Data.Member
                }
            );
        }

        private static ClassMap<T> GetConfiguration<T>()
        {
            var conf = new Configuration();

            var opt1 = conf.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] {"O"};

            conf.TypeConverterCache.AddConverter(typeof(Guid), new GuidConverter());

            var map = conf.AutoMap<T>();

            return map;
        }
    }
}