using System.Linq;
using Xunit;

namespace FastInsert.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void EnumerableShouldBeProperlyPartitioned()
        {
            var list = new[] {1, 2, 3, 4, 5};

            var partitioned = EnumerableExtensions.GetPartitions(list, partitionSize: 2);
                
            var traversed = partitioned.SelectMany(p => p);

            Assert.Equal(list, traversed);
        }
    }
}
