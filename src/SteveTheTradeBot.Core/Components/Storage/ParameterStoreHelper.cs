using System;
using System.Globalization;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Utils;
using static System.Boolean;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class StrategyProperty
    {
        public const string UpdateStopLossAt = "UpdateStopLossAt";
        public const string BoughtAtPrice = "BoughtAtPrice";
        public const string StopLoss = "StopLoss";
        public const string Risk = "Risk";
        public const string ExitAt = "ExitAt";
    }

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

        public static async Task<bool> Get(this IParamsStoreSimple data, string key, bool value)
        {
            var get = await data.Get(key, null);
            if (TryParse(get, out var result))
            {
                return result;
            }
            return value;
        }

        public static Task Set(this IParamsStoreSimple data, string key, decimal value)
        {
            return data.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static Task Set(this IParamsStoreSimple data, string key, in DateTime value)
        {
            return data.Set(key, value.ToIsoDateString());
        }

        public static Task Set(this IParamsStoreSimple data, string key, in bool value)
        {
            return data.Set(key, value.ToString());
        }
    }
}