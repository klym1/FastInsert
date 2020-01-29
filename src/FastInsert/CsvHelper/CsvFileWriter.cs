using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace FastInsert.CsvHelper
{
    public class CsvFileWriter : ICsvWriter
    {
        private readonly ClassMap _classMap;

        public CsvFileWriter(ClassMap classMap) => _classMap = classMap;

        public async Task WriteAsync<T>(IEnumerable<T> list, CsvFileSettings settings)
        {
            using var fileStream = new FileStream(settings.Path, FileMode.Create);
            using var textWriter = new StreamWriter(fileStream);
            using var writer = new CsvWriter(new CsvSerializer(textWriter, CultureInfo.CurrentCulture));
            writer.Configuration.Delimiter = settings.Delimiter;
            writer.Configuration.RegisterClassMap(_classMap);
            await writer.WriteRecordsAsync(list);
        }
    }
}
