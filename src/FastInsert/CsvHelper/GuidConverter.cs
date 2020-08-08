
using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using static FastInsert.BinaryFormat;

namespace FastInsert.CsvHelper
{
    public class GuidConverter : DefaultTypeConverter
    {
        private readonly BinaryFormat _binaryFormat;

        public GuidConverter(BinaryFormat format) 
            => _binaryFormat = format;

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var guid = (Guid) value;
            var str = guid.ToString("N");

            return _binaryFormat switch
            {
                Base64 => Convert.ToBase64String(Converter.StringToByteArray(str)),
                Hex => str,
                _ => throw new ArgumentOutOfRangeException(nameof(_binaryFormat))
            };
        }
    }
}
