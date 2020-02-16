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
        
        [Fact]
        public async Task BinaryColumnTest()
        {
            using var connection = GetConnection();
            var tableName = "test_binary_column";
            var list = new[]
            {
                new TableWithBinaryColumn
                {
                    Bytes = new byte[48]
                }
            };

            new Random().NextBytes(list[0].Bytes);
            
            await connection.ExecuteAsync($"drop table if exists {tableName}");
            await connection.ExecuteAsync($@"
                CREATE TABLE IF NOT EXISTS `{tableName}` (
                  `bytes` binary(48) NOT NULL
                  );  ");

            _testOutputHelper.WriteLine("Table created");

            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<TableWithBinaryColumn>($"select * from {tableName}")).ToList();

            Assert.Equal(list[0].Bytes, actualData[0].Bytes);
        }
        
        [Fact]
        public async Task NullableGuidTest()
        {
            using var connection = GetConnection();
            var tableName = "NullableGuidTest";
            var list = new[]
            {
                new NullableGuid
                {
                    Guid = null
                },
                new NullableGuid
                {
                    Guid = Guid.Empty
                }
            };
            
            await connection.ExecuteAsync($"drop table if exists {tableName}");
            await connection.ExecuteAsync($@"
                CREATE TABLE IF NOT EXISTS `{tableName}` (
                  `Guid` binary(48) NULL
                  );  ");

            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<NullableGuid>($"select * from {tableName}")).ToList();

            Assert.Equal(list[0].Guid, actualData[0].Guid);
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
        
        private class NullableGuid
        {
            public Guid? Guid { get; set; }
        }
        
        private class TableWithBinaryColumn
        {
            public byte[] Bytes { get; set; }
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
