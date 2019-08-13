using System.Collections.Generic;

namespace FastInsert
{
    public class FieldsExpressionBuilder
    {
        public static string ToExpression(TableDef tableDef)
        {
            var fields = new List<string>();
            var transformations = new List<string>();
            var transformColumnIndex = 0;

            foreach (var col in tableDef.Columns)
            {
                if (col.RequiresTransformation)
                {
                    var var = $"@var{transformColumnIndex++}";

                    fields.Add(var);
                    transformations.Add($"SET `{col.Name}` = {col.TransformFunc(var)}");
                }
                else
                {
                    fields.Add($"`{col.Name}`");
                }
            }

            var joinedFields = string.Join(", ", fields);
            var joinedTransformations = string.Join("\n", transformations);

            return $"({joinedFields})\n{joinedTransformations}";
        }
    }
}