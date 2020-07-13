using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert.CsvHelper
{
    public class GuidConverter : ITypeConverter
    {
        private readonly ByteArrayConverterOptions _byteArrayOptions;

        public GuidConverter(ByteArrayConverterOptions byteArrayOptions) 
            => _byteArrayOptions = byteArrayOptions;

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var guid = (Guid?) value;

            if (guid == null)
                return "\\N";
            
            var str = guid.Value.ToString("N");

            if (_byteArrayOptions == ByteArrayConverterOptions.Base64)
            {
                var bytes = Converter.StringToByteArray(str);
                return Convert.ToBase64String(bytes);
            }
            
            return str;
        }
        
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) 
            => throw new NotImplementedException();
    }
}
