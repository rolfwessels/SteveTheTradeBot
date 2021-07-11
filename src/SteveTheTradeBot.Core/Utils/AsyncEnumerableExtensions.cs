using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Utils
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items,
            CancellationToken cancellationToken = default)
        {
            var results = new List<T>();
            await foreach (var item in items.WithCancellation(cancellationToken)
                .ConfigureAwait(false))
                results.Add(item);
            return results;
        }

        public static async IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> items,
            int count, CancellationToken cancellationToken = default)
        {
            var counter = 0;
            await foreach (var item in items.WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                if (counter == count) yield break;
                counter++;
                yield return item;
            }
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }

        
}
}