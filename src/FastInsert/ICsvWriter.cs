using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastInsert
{
    public interface ICsvWriter
    {
        Task WriteAsync<T>(IEnumerable<T> list, CsvFileSettings settings);
    }
}
