using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert.CsvHelper
{
    public class NullConverter : DefaultTypeConverter
    {
        private readonly ITypeConverter _underlyingTypeConverter;

        public NullConverter(Type type, TypeConverterCache typeConverterFactory)
        {
            _underlyingTypeConverter = typeConverterFactory.GetConverter(type);
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            => value == null
                ? "\\N"
                : _underlyingTypeConverter.ConvertToString(value, row, memberMapData);
    }
}
