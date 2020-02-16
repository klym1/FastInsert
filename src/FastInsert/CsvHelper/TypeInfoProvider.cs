using System;
using System.Collections.Generic;
using System.Linq;

namespace FastInsert.CsvHelper
{
    public static class TypeInfoProvider
    {
        public static IEnumerable<CsvColumnDef> GetClassFields(Type type)
        {
            return ClassAutoMapper.AutoMap(type).MemberMaps.Where(m => !m.Data.Ignore).Select(m => new CsvColumnDef
                {
                    Name = m.Data.Names[0],
                    MemberInfo = m.Data.Member
                }
            );
        }
    }
}
