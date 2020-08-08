using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert.CsvHelper
{
    public class NullableConverter : global::CsvHelper.TypeConversion.NullableConverter
    {
        public NullableConverter(Type type, TypeConverterCache typeConverterFactory) : base(type, typeConverterFactory)
        {
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            => value == null
                ? "\\N"
                : base.ConvertToString(value, row, memberMapData);
    }
}
