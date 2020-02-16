using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert.CsvHelper
{
    public class CsvWriterConfigurator
    {
        public static ICsvWriter GetWriter(Type type)
        {
            return new CsvFileWriter(GetConfiguration(type));
        }
        
        private static ClassMap GetConfiguration(Type type)
        {
            var conf = new CsvConfiguration(CultureInfo.CurrentCulture);

            var opt1 = conf.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] {"O"};

            conf.TypeConverterCache.AddConverter(typeof(Guid), new GuidConverter());;

            const ByteArrayConverterOptions byteArrayConverterOptions = ByteArrayConverterOptions.Hexadecimal;
            conf.TypeConverterCache.AddConverter(typeof(byte[]), new ByteArrayConverter(byteArrayConverterOptions));
            
            return ClassAutoMapper.AutoMap(type, conf);
        }
    }
}
