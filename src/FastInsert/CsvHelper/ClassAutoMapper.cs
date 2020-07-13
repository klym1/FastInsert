using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;

namespace FastInsert.CsvHelper
{
    public static class ClassAutoMapper
    {
        public static ClassMap AutoMap(Type type, CsvConfiguration? conf = null)
        {
            var map = (conf ?? new CsvConfiguration(CultureInfo.CurrentCulture)).AutoMap(type);
            
            map.MemberMaps
                .Where(m => !((m.Data.Member as PropertyInfo)?.CanWrite ?? true))
                .ToList()
                .ForEach(m => m.Ignore());

            return map;
        }
    }
}
