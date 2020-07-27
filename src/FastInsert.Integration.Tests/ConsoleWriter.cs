using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace FastInsert.Integration.Tests
{
    public class ConsoleWriter : TextWriter
    {
        public override Encoding Encoding { get; }

        private readonly ITestOutputHelper _helper;

        public ConsoleWriter(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        public override void WriteLine(string value)
        {
            _helper.WriteLine(value);
        }
    }
}
