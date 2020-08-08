using System;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace FastInsert.CsvHelper
{
    public class CsvWriterConfigurator
    {
        public static ICsvWriter GetWriter(Type type, BinaryFormat binaryFormat)
        {
            return new CsvFileWriter(GetConfiguration(type, binaryFormat));
        }
        
        private static ClassMap GetConfiguration(Type type, BinaryFormat binaryFormat)
        {
            var conf = new CsvConfiguration(CultureInfo.CurrentCulture);

            var opt1 = conf.TypeConverterOptionsCache.GetOptions<DateTime>();
            opt1.DateTimeStyle = DateTimeStyles.AssumeUniversal;
            opt1.Formats = new[] {"O"};

            var byteArrayOptions = GetOptions(binaryFormat);
            
            conf.TypeConverterCache.AddConverter(typeof(Guid), new GuidConverter(binaryFormat));
            conf.TypeConverterCache.AddConverter(typeof(byte[]), new ByteArrayConverter(byteArrayOptions));

            type.GetProperties()
                .Select(it => it.PropertyType)
                .ToList()
                .ForEach(prop =>
                {
                    if (prop.IsNullable(out _))
                    {
                        conf.TypeConverterCache.AddConverter(prop, new NullableConverter(prop, conf.TypeConverterCache));
                    }

                    if (prop.IsEnum || prop.IsNullableEnum())
                    {
                        var opts = conf.TypeConverterOptionsCache.GetOptions(prop);
                        opts.Formats = new[] {"D"};
                    }
                });
                
            return ClassAutoMapper.AutoMap(type, conf);
        }

        private static ByteArrayConverterOptions GetOptions(BinaryFormat binaryFormat) =>
            binaryFormat switch
            {
                BinaryFormat.Hex => ByteArrayConverterOptions.Hexadecimal,
                _ => ByteArrayConverterOptions.Base64
            };
    }
}
