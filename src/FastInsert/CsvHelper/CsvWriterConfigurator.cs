using System;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert.CsvHelper
{
    public class CsvWriterConfigurator
    {
        public static ICsvWriter GetWriter<T>()
        {
            return new CsvFileWriter(GetConfiguration<T>());
        }
        
        private static ClassMap<T> GetConfiguration<T>()
        {
            var conf = new CsvConfiguration(CultureInfo.CurrentCulture);

            var opt1 = conf.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] {"O"};

            conf.TypeConverterCache.AddConverter(typeof(Guid), new GuidConverter());;

            const ByteArrayConverterOptions byteArrayConverterOptions = ByteArrayConverterOptions.Hexadecimal;
            conf.TypeConverterCache.AddConverter(typeof(byte[]), new ByteArrayConverter(byteArrayConverterOptions));
            
            var map = conf.AutoMap<T>();
            return map;
        }
    }
}
