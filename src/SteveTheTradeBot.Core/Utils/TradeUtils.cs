using System;
using System.Collections.Generic;
using System.Linq;
using BetterConsoleTables;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Utils
{
    public static class TradeUtils
    {
        public static decimal MovementPercent(decimal currentValue, decimal fromValue, int decimals = 3)
        {
            if (fromValue == 0) fromValue = 0.00001m;
            return Math.Round((currentValue - fromValue) / fromValue * 100, decimals);
        }

        public static string ToTable<T>(this IEnumerable<T> enumerable)
        {
            var table = new Table().From(enumerable.ToList()).With(x => x.Config = TableConfiguration.UnicodeAlt());
            if (!table.Rows.Any()) return "No records found.\n";
            return new ConsoleTables(table).ToString();
        }


        public static TradeFeedCandle ForDate(this IEnumerable<TradeFeedCandle> updateFeed )
        {
            return ForDate(updateFeed, DateTime.Now);
        }

        public static TradeFeedCandle ForDate(this IEnumerable<TradeFeedCandle> updateFeed,
            DateTime dateTime )
        {
            foreach (var tradeFeedCandle in updateFeed)
            {
                if (dateTime.ToUniversalTime() >= tradeFeedCandle.Date.ToUniversalTime() && 
                    dateTime.ToUniversalTime() < tradeFeedCandle.Date.ToUniversalTime().Add(tradeFeedCandle.PeriodSize.ToTimeSpan())) 
                    return tradeFeedCandle;
            }
            return null;
        }

        public static void Print(this StrategyInstance backTestResult)
        {
            Console.Out.WriteLine("BalanceMoved: " + backTestResult.PercentProfit);
            Console.Out.WriteLine("MarketMoved: " + backTestResult.PercentMarketProfit);
            Console.Out.WriteLine("Trades: " + backTestResult.TotalNumberOfTrades);
            Console.Out.WriteLine("TradesSuccesses: " + backTestResult.NumberOfProfitableTrades);
            Console.Out.WriteLine("TradesSuccessesPercent: " + backTestResult.PercentOfProfitableTrades);
            Console.Out.WriteLine("TradesActive: " + backTestResult.TotalActiveTrades);
            Console.Out.WriteLine("AvgDuration: " + backTestResult.AverageTimeInMarket.ToShort());
            Console.Out.WriteLine("AverageTradesPerMonth: " + backTestResult.AverageTradesPerMonth);
            var tradeValues = backTestResult.Trades
                .Select(x => new
                {
                    x.StartDate, x.Profit, Value = x.SellValue - x.BuyValue,
                    MarketMoved = TradeUtils.MovementPercent(x.SellPrice, x.BuyPrice)
                })
                .OrderByDescending(x => x.Value).ToArray();
            Console.Write(tradeValues.Take(10).Concat(tradeValues.TakeLast(10)).ToTable().ToString());
            Console.Write(backTestResult.Trades
                .Select(x => new {x.StartDate, x.BuyValue, Quantity = x.BuyQuantity, x.BuyPrice, x.SellPrice}).ToTable()
                .ToString());
            Console.Write(backTestResult.Trades.SelectMany(x => x.Orders).Select(x =>
                    new {x.OrderSide, x.PriceAtRequest, x.OrderPrice, x.OutQuantity, x.OriginalQuantity, x.CurrencyPair})
                .ToTable()
                .ToString());
        }
    }
}