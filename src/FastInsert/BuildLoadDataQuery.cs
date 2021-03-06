﻿using System.Collections.Generic;

namespace FastInsert
{
    internal static class BuildLoadDataQuery
    {
        public static string BuildQuery(string tableName, List<Column> tableDef, CsvFileSettings settings)
        {
            var fieldsExpression = FieldsExpressionBuilder.ToExpression(tableDef);

            var lines = new[]
            {
                $"LOAD DATA LOCAL INFILE '{settings.Path}'",
                $"INTO TABLE {tableName}",
                $"COLUMNS TERMINATED BY '{settings.Delimiter}' ENCLOSED BY '{settings.FieldEnclosedByChar}' ESCAPED BY '{settings.FieldEscapedByChar}'",
                $"LINES TERMINATED BY '{settings.LineEnding}' STARTING BY ''",
                $"IGNORE 1 LINES",
                fieldsExpression
            };

            return string.Join("\n", lines);
        }
    }
}
