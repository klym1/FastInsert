using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using FastInsert;

static internal class CsvFileWriter
{
    public static void WriteToCsvFileAsync<T>(ClassMap classMap, IEnumerable<T> list, CsvFileSettings settings)
    {
        using var fileStream = new FileStream(settings.Path, FileMode.Create);
        using var textWriter = new StreamWriter(fileStream);
        using var writer = new CsvWriter(textWriter);
        writer.Configuration.Delimiter = settings.Delimiter;
        writer.Configuration.RegisterClassMap(classMap);

        writer.WriteRecords(list);
    }
}