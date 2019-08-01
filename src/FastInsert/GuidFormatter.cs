using System;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert
{
    public class GuidFormatter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var bytes = ((Guid)value).ToByteArray();
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }
    }
}