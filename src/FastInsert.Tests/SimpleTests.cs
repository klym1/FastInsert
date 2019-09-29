using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Xunit;
using Xunit.Abstractions;

namespace FastInsert.Tests
{
    public class SimpleTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SimpleTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task GeneratedDataIsCorrectlyInserted()
        {
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());

            var connBuilder = new MySqlConnectionStringBuilder
            {
                AllowLoadLocalInfile = true,
                AllowUserVariables = true,
                Database = "tests",
                UserID = "test",
                Password = "pass"
            };

            var conn = connBuilder.ToString();
            var connection = new MySqlConnection(conn);

            var list = GenerateData().ToList();

            await connection.ExecuteAsync("drop table if exists test");
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS `test` (
                  `guid` binary(16) NOT NULL,
                  `dateCol` datetime(3) NOT NULL,
                  `int` int NOT NULL,
                  `text` text NOT NULL
                  );  ");

            await connection.FastInsertAsync(list, "test");

            _testOutputHelper.WriteLine("After insert");

            var rows = await connection.ExecuteScalarAsync<int>("select count(*) from test");

            _testOutputHelper.WriteLine($"Number of rows: {rows}");

            var reader = await connection.ExecuteReaderAsync("select * from test");

            var i = 0;

            while (await reader.ReadAsync())
            {
                _testOutputHelper.WriteLine($"col {i}: {reader[i]}");
                i++;
            }

            var actualData = (await connection.QueryAsync<Table>("select * from test")).ToList();

            Assert.Equal(list[0].DateCol, actualData[0].DateCol, TimeSpan.FromMilliseconds(1));
            Assert.Equal(list[0].Guid, actualData[0].Guid);
            Assert.Equal(list[0].Int, actualData[0].Int);
            Assert.Equal(list[0].Text, actualData[0].Text);
        }

        private static IEnumerable<Table> GenerateData()
        {
            return Enumerable.Range(1, 1)
                .Select(it =>
                    new Table
                    {
                        Int = it,
                        Text = "text" + it,
                        DateCol = DateTime.UtcNow.AddHours(it),
                        Guid = Guid.NewGuid()
                    });
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
