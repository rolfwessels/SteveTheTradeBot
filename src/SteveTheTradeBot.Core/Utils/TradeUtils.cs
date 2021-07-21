using System;
using System.Collections.Generic;
using System.Linq;
using BetterConsoleTables;

namespace SteveTheTradeBot.Core.Utils
{
    public static class TradeUtils
    {
        public static decimal MovementPercent(decimal currentValue, decimal fromValue, int decimals = 3)
        {
            if (fromValue == 0) fromValue = 0.00001m;
            return Math.Round((currentValue - fromValue) / fromValue * 100, decimals);
        }

        public static ConsoleTables ToTable<T>(IEnumerable<T> enumerable)
        {
            var table = new Table().From(enumerable.ToList()).With(x => x.Config = TableConfiguration.UnicodeAlt());
            return new ConsoleTables(table);
        }
    }
}