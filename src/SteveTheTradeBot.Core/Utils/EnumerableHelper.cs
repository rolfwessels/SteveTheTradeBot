using System;
using System.Collections.Generic;

namespace SteveTheTradeBot.Core.Utils
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> ForAll<T>(this IEnumerable<T> values, Action<T> call)
        {
            if (values == null)
                yield break;
            foreach (T obj in values)
            {
                call(obj);
                yield return obj;
            }
        }
    }
}