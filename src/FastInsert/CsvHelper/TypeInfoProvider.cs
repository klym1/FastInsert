using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;

namespace FastInsert.CsvHelper
{
    public static class TypeInfoProvider
    {
        public static IEnumerable<CsvColumnDef> GetClassFields(Type type)
        {
            var conf = new CsvConfiguration(CultureInfo.CurrentCulture);
            var map = conf.AutoMap(type);

            return map.MemberMaps.Select(m => new CsvColumnDef
                {
                    Name = m.Data.Names[0],
                    MemberInfo = m.Data.Member
                }
            );
        }
    }
}
