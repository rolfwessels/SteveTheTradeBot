using System;
using System.Collections.Generic;
using System.Linq;

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
        public static IEnumerable<T> PrintTable<T>(this IEnumerable<T> values)
        {
            var printTable = values as T[] ?? values.ToArray();
            Console.Out.WriteLine(printTable.ToTable());
            return printTable;
        } 
        public static Dictionary<T, T2> AddOrReplace<T,T2>(this Dictionary<T,T2> values, Dictionary<T, T2> call)
        {
            foreach (var keyValuePair in call)
            {
                values.AddOrReplace(keyValuePair.Key, keyValuePair.Value);
            }
            return values;
        }

        public static Dictionary<T, T2> AddOrReplace<T, T2>(this Dictionary<T, T2> values, T key, T2 value)
        {
            if (!values.ContainsKey(key))
            {
                values.Add(key, value);
            }
            else
            {
                values[key] = value;
            }

            return values;
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