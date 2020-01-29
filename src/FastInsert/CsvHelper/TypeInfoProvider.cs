using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;

namespace FastInsert.CsvHelper
{
    public static class TypeInfoProvider
    {
        public static IEnumerable<CsvColumnDef> GetClassFields<T>()
        {
            var conf = new CsvConfiguration(CultureInfo.CurrentCulture);
            var map = conf.AutoMap<T>();

            return map.MemberMaps.Select(m => new CsvColumnDef
                {
                    Name = m.Data.Names[0],
                    MemberInfo = m.Data.Member
                }
            );
        }
    }
}
