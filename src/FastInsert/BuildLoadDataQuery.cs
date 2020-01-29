using FastInsert;

internal static class BuildLoadDataQuery
{
    public static string BuildQuery(string tableName, TableDef tableDef, CsvFileSettings settings)
    {
        var fieldsExpression = FieldsExpressionBuilder.ToExpression(tableDef);

        return $@"LOAD DATA LOCAL INFILE '{settings.Path}' 
                   INTO TABLE {tableName} 
                    COLUMNS TERMINATED BY '{settings.Delimiter}' ENCLOSED BY '{settings.FieldEnclosedByChar}' ESCAPED BY '{settings.FieldEscapedByChar}'
                    LINES TERMINATED BY '{settings.LineEnding}' STARTING BY ''
                    IGNORE 1 LINES                    
                    {fieldsExpression}
                    ";
    }
}
