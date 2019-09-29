using System;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;

namespace FastInsert.Tests
{
    public class BaseTests
    {
        public BaseTests()
        {
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());
        }

        public IDbConnection GetConnection()
        {
            var connBuilder = new MySqlConnectionStringBuilder
            {
                AllowLoadLocalInfile = true,
                AllowUserVariables = true,
                Database = "tests",
                UserID = "test",
                Password = "pass"
            };

            var conn = connBuilder.ToString();
            return new MySqlConnection(conn);
        }
    }
}