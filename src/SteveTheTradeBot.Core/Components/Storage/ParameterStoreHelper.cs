using System;
using System.Globalization;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public static class ParameterStoreHelper
    {
        public static async Task<decimal> Get(this IParamsStoreSimple data, string key, decimal value)
        {
            var get = await data.Get(key, null);    
            if (decimal.TryParse(get, out var result))
            {
                return result;
            }
            return value;
        }

        public static async Task<DateTime> Get(this IParamsStoreSimple data, string key, DateTime defaultValue)
        {
            var value = await data.Get(key, "_");
            if (value != "_" && DateTime.TryParse(value, out var date))
            {
                return date;
            }
            return defaultValue;
        }

        public static Task Set(this IParamsStoreSimple data, string key, decimal value)
        {
            return data.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }


        public static Task Set(this IParamsStoreSimple data, string key, in DateTime value)
        {
            return data.Set(key, value.ToIsoDateString());
        }
    }
}