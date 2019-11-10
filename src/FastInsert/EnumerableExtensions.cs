using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FastInsert
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> GetPartitions<T>(IEnumerable<T> list, int partitionSize)
        {
            if (partitionSize < 1)
                throw new ArgumentOutOfRangeException(nameof(partitionSize), partitionSize, "Partition size expected to be positive");

            return new PartitionHelper<T>(list, partitionSize);
        }

        private sealed class PartitionHelper<T> : IEnumerable<IEnumerable<T>>
        {
            private readonly IEnumerable<T> _items;
            private readonly int _partitionSize;
            private bool _hasMoreItems;

            internal PartitionHelper(IEnumerable<T> i, int ps)
            {
                _items = i;
                _partitionSize = ps;
            }

            public IEnumerator<IEnumerable<T>> GetEnumerator()
            {
                using var enumerator = _items.GetEnumerator();
                _hasMoreItems = enumerator.MoveNext();
                while (_hasMoreItems)
                    yield return GetNextBatch(enumerator).ToList();
            }

            private IEnumerable<T> GetNextBatch(IEnumerator<T> enumerator)
            {
                for (var i = 0; i < _partitionSize; ++i)
                {
                    yield return enumerator.Current;
                    _hasMoreItems = enumerator.MoveNext();
                    if (!_hasMoreItems)
                        yield break;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}