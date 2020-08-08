using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xunit;
using Xunit.Abstractions;

namespace FastInsert.Integration.Tests
{
    public class SimpleTests : BaseTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SimpleTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        [WithTable(@"             
                  `int32` int NOT NULL
                 , `mediumText` mediumText NOT NULL"
        )]
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

            const string tableName = nameof(InsertAllTheDataInSeveralBatches);

            await connection.FastInsertAsync(list, o => o
                .BatchSize(2)
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper))
            );

            var actualNumberOfRows = await connection.ExecuteScalarAsync<int>($"select count(*) from {tableName}");

            Assert.Equal(list.Count, actualNumberOfRows);
        }

        [Fact]
        [WithTable(@"
                  `guid`    binary(16)  NOT NULL
                , `dateCol` datetime(3) NOT NULL
                , `int`     int         NOT NULL
                , `text`    text        NOT NULL"
        )]
        public async Task GeneratedDataIsCorrectlyInserted()
        {
            using var connection = GetConnection();
            var list = GenerateData().ToList();

            const string table = nameof(GeneratedDataIsCorrectlyInserted); 
            
            await connection.FastInsertAsync(list, o => o
                .ToTable(table)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<Table>($"select * from {table}")).ToList();

            Assert.Equal(list[0].DateCol, actualData[0].DateCol, TimeSpan.FromMilliseconds(1));
            Assert.Equal(list[0].Guid, actualData[0].Guid);
            Assert.Equal(list[0].Int, actualData[0].Int);
            Assert.Equal(list[0].Text, actualData[0].Text);
        }

        [Fact]
        [WithTable("`bytes` binary(48)")]
        public async Task BinaryColumnTest()
        {
            using var connection = GetConnection();
            const string tableName = nameof(BinaryColumnTest);
            var list = new[]
            {
                new TableWithBinaryColumn
                {
                    Bytes = new byte[48]
                },
                new TableWithBinaryColumn
                {
                    Bytes = null
                }
            };

            new Random().NextBytes(list[0].Bytes);
            
            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<TableWithBinaryColumn>($"select * from {tableName}")).ToList();

            Assert.Equal(list[0].Bytes, actualData[0].Bytes);
            Assert.Equal(list[1].Bytes, actualData[1].Bytes);
        }
        
        [Fact]
        [WithTable("`bytes` binary(48) NOT NULL")]
        public async Task BinaryColumnWithBase64Test()
        {
            using var connection = GetConnection();
            const string tableName = nameof(BinaryColumnWithBase64Test);
            var list = new[]
            {
                new TableWithBinaryColumn
                {
                    Bytes = new byte[48]
                }
            };

            new Random().NextBytes(list[0].Bytes);
            
            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .BinaryFormat(BinaryFormat.Hex)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<TableWithBinaryColumn>($"select * from {tableName}")).ToList();

            Assert.Equal(list[0].Bytes, actualData[0].Bytes);
        }
        
        [Fact]
        [WithTable("`Guid` binary(48) NULL")]
        public async Task NullableGuidTest()
        {
            using var connection = GetConnection();
            const string tableName = nameof(NullableGuidTest);
            var list = new[]
            {
                new NullableGuid
                {
                    Guid = null
                },
                new NullableGuid
                {
                    Guid = Guid.Empty
                },
                new NullableGuid
                {
                    Guid = Guid.Parse("885DD3E8-A733-4597-AE84-652E85E4DECD")
                }
            };
            
            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<NullableGuid>($"select * from {tableName}")).ToList();
            
            Assert.Equal(list[0].Guid, actualData[0].Guid);
            Assert.Equal(list[1].Guid, actualData[1].Guid);
            Assert.Equal(list[2].Guid, actualData[2].Guid);
        }
        
        [Fact]
        [WithTable("`Val` text")]
        public async Task WithoutGetterTest()
        {
            using var connection = GetConnection();
            const string tableName = nameof(WithoutGetterTest);
            var list = new[]
            {
                new WithoutGetter
                {
                    Val = "123"
                },
            };
            
            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<WithoutGetter>($"select * from {tableName}")).ToList();

            Assert.Equal(list[0].Val, actualData[0].Val);
            Assert.Equal(list[0].Descr, actualData[0].Descr);
        }
        
        [Fact]
        [WithTable(@"`Val1` int not null, `NullableVal` int")]
        public async Task EnumTest()
        {
            using var connection = GetConnection();
            const string tableName = nameof(EnumTest);
            var list = new[]
            {
                new WithEnum
                {
                    Val1 = TestEnum.Three,
                    NullableVal = TestEnum.Two,
                },
                
                new WithEnum
                {
                    Val1 = TestEnum.Two,
                    NullableVal = null,
                },
            };
            
            await connection.FastInsertAsync(list, o => o
                .ToTable(tableName)
                .Writer(new ConsoleWriter(_testOutputHelper)));

            var actualData = (await connection.QueryAsync<WithEnum>($"select * from {tableName}")).ToList();

            Assert.Equal(list[0].Val1, actualData[0].Val1);
            Assert.Equal(list[0].NullableVal, actualData[0].NullableVal);
            
            Assert.Equal(list[1].Val1, actualData[1].Val1);
            Assert.Equal(list[1].NullableVal, actualData[1].NullableVal);
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
        
        private class WithEnum
        {
            public TestEnum Val1 { get; set; }
            public TestEnum? NullableVal { get; set; }
        }

        private enum TestEnum
        {
            One, Two, Three
        }
        
        private class WithoutGetter
        {
            public string Val { get; set; }
            public string Descr => "Description: " + Val;
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
}
