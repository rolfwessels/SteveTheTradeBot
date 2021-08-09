using System;
using System.Collections.Generic;
using System.Linq;
using BetterConsoleTables;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBotML.Model;

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

        public static TradeQuote ForDate(this IEnumerable<TradeQuote> updateFeed )
        {
            return ForDate(updateFeed, DateTime.Now);
        }

        public static TradeQuote ForDate(this IEnumerable<TradeQuote> updateFeed,
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
            Console.Out.WriteLine("Reference: " + backTestResult.Reference);
            Console.Out.WriteLine("BalanceMoved: " + backTestResult.PercentProfit);
            Console.Out.WriteLine("MarketMoved: " + backTestResult.PercentMarketProfit);
            Console.Out.WriteLine("Trades: " + backTestResult.TotalNumberOfTrades);
            Console.Out.WriteLine("TradesSuccesses: " + backTestResult.NumberOfProfitableTrades);
            Console.Out.WriteLine("TradesSuccessesPercent: " + backTestResult.PercentOfProfitableTrades);
            Console.Out.WriteLine("TradesActive: " + backTestResult.TotalActiveTrades);
            Console.Out.WriteLine("TotalFee: " + backTestResult.TotalFee);
            Console.Out.WriteLine("AvgDuration: " + backTestResult.AverageTimeInMarket.ToShort());
            Console.Out.WriteLine("AverageTradesPerMonth: " + backTestResult.AverageTradesPerMonth);
            
            Console.Write(backTestResult.Trades
                .Select(x => new {x.StartDate, x.BuyValue, Quantity = x.BuyQuantity, x.BuyPrice, x.SellPrice , x.Profit , x.FeeAmount}).ToTable()
                .ToString());
            Console.Write(backTestResult.Trades.SelectMany(x => x.Orders).Select(x =>
                    new {x.OrderSide, x.OrderStatusType, x.OrderType, x.PriceAtRequest, x.OrderPrice,
                        OutQuantity = x.Total, x.OriginalQuantity, x.CurrencyPair,FeeAmount = x.TotalFee})
                .ToTable()
                .ToString());
        }

       
    }
}