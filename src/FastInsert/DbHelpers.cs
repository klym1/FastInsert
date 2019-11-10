using System.Collections.Generic;
using System.Data;

static internal class DbHelpers
{
    public static IEnumerable<string> GetTableColumns(IDbConnection connection, string tableName, string dbName)
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