using System;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert
{
    public class GuidConverter : ITypeConverter
    {
        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var guid = (Guid) value;

            var bytes = guid.ToByteArray();

            var flippedBytes = FlipEndian(bytes);
            var formattedBytes = FormatBytes(flippedBytes);
            
            return formattedBytes;
        }

        internal static string FormatBytes(byte[] bytes)
        {
            return string.Join("", bytes.Select(b => b.ToString("x2")));
        }

        internal static byte[] FlipEndian(byte[] oldBytes)
        {
            var newBytes = new byte[16];
            for (var i = 8; i < 16; i++)
                newBytes[i] = oldBytes[i];

            newBytes[3] = oldBytes[0];
            newBytes[2] = oldBytes[1];
            newBytes[1] = oldBytes[2];
            newBytes[0] = oldBytes[3];
            newBytes[5] = oldBytes[4];
            newBytes[4] = oldBytes[5];
            newBytes[6] = oldBytes[7];
            newBytes[7] = oldBytes[6];

            return newBytes;
        }

        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            throw new NotImplementedException();
        }
    }
}