using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Xunit;
using Xunit.Abstractions;

namespace FastInsert.Tests
{
    public class SimpleTests : BaseTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SimpleTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task InsertAllTheDataInSeveralBatches()
        {
            using var connection = GetConnection();
            var list = Enumerable.Range(1, 10)
                .Select(it =>
                    new Test123
                    {
                        Int32 = it,
                        MediumText = "text" + it,
                    }).ToList();

            var tableName = "Test123";

            await connection.ExecuteAsync($"drop table if exists `{tableName}`");
            await connection.ExecuteAsync($@"
                CREATE TABLE IF NOT EXISTS `{tableName}` (                 
                  `int32` int NOT NULL,
                  `mediumText` mediumText NOT NULL
                  );  ");

            await connection.FastInsertAsync(list, o => o
                .BatchSize(2)
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper))
            );

            var actualNumberOfRows = await connection.ExecuteScalarAsync<int>($"select count(*) from {tableName}");

            Assert.Equal(list.Count, actualNumberOfRows);
        }

        [Fact]
        public async Task GeneratedDataIsCorrectlyInserted()
        {
            using var connection = GetConnection();
            var list = GenerateData().ToList();

            await connection.ExecuteAsync("drop table if exists test");
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS `test` (
                  `guid` binary(16) NOT NULL,
                  `dateCol` datetime(3) NOT NULL,
                  `int` int NOT NULL,
                  `text` text NOT NULL
                  );  ");

            _testOutputHelper.WriteLine("Table created");

            await connection.FastInsertAsync(list, o => o
                .ToTable("test")
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<Table>("select * from test")).ToList();

            Assert.Equal(list[0].DateCol, actualData[0].DateCol, TimeSpan.FromMilliseconds(1));
            Assert.Equal(list[0].Guid, actualData[0].Guid);
            Assert.Equal(list[0].Int, actualData[0].Int);
            Assert.Equal(list[0].Text, actualData[0].Text);
        }
        
        private static IEnumerable<Table> GenerateData()
        {
            return Enumerable.Range(1, 100)
                .Select(it =>
                    new Table
                    {
                        Int = it,
                        Text = "text" + it,
                        DateCol = DateTime.UtcNow.AddHours(it),
                        Guid = Guid.NewGuid()
                    });
        }

        private class Test123
        {
            public int Int32 { get; set; }
            public string MediumText { get; set; }
        }

        private class Table
        {
            public Guid Guid { get; set; }
            public DateTime DateCol { get; set; }
            public int Int { get; set; }
            public string Text { get; set; }
        }
    }

    public class ConsoleWriter : TextWriter
    {
        public override Encoding Encoding{ get; }

        private ITestOutputHelper _helper;

        public ConsoleWriter(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        public override void WriteLine(string value)
        {
            _helper.WriteLine(value);
        }
    }
}
