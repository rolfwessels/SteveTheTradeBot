using System;
using System.Collections.Generic;
using System.Linq;
using Bumbershoot.Utilities.Helpers;

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

        public static IEnumerable<List<T>> BatchedBy<T>(this IEnumerable<T> source, int size = 1000)
        {
            var counter = 0;
            var list = new List<T>();
            foreach (var val in source)
            {
                if (counter >= size)
                {
                    yield return list;
                    list = new List<T>();
                    counter = 0;
                }
                list.Add(val);
                counter++;
            }

            if (list.Count > 0)
            {
                yield return list;
            }
        }
    }
}