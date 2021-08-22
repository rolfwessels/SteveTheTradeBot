using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;
using Bumbershoot.Utilities.Helpers;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public static class Signals
    {
        public const string Ema200 = "ema200";
        
        public const string Rsi14 = "rsi14";
        public const string Ema100 = "ema100";
        public const string Roc100 = "roc100";
        public const string Roc200 = "roc200";
        public const string Roc100sma = "roc100-sma";
        public const string Roc200sma = "roc200-sma";
        public const string Supertrend = "supertrend";
        
        public const string MacdValue = "macd";
        public const string MacdSignal = "macd-signal";
        public const string MacdHistogram = "macd-histogram";

        public const string SuperTrendLower = "supertrend-lower";
        public const string SuperTrendUpper = "supertrend-upper";

        public static class Macd
        {
            public static List<TradeQuote> GetCrossedMacdOverSignal(IEnumerable<TradeQuote> takeLast)
            {
                return GetCrossed(takeLast, MacdValue, MacdSignal);
            }
            
            public static List<TradeQuote> GetCrossedSignalOverMacd(IEnumerable<TradeQuote> takeLast)
            {
                return GetCrossed(takeLast, MacdSignal, MacdValue);
            }

            private static List<TradeQuote> GetCrossed(IEnumerable<TradeQuote> takeLast, string value, string overSignal)
            {
                var tradeQuotes = takeLast.ToList();
                var result = new List<TradeQuote>();
                for (int i = 1; i < tradeQuotes.Count(); i++)
                {
                    var prev = tradeQuotes[i - 1];
                    var current = tradeQuotes[i];
                    if (prev.Metric.GetOrDefault(value) <= prev.Metric.GetOrDefault(overSignal))
                    {
                        if (current.Metric.GetOrDefault(value) > current.Metric.GetOrDefault(overSignal))
                        {
                            result.Add(current);
                        }
                    }
                }
                return result;
            }


            public static bool IsCrossedBelowZero(IEnumerable<TradeQuote> crossed)
            {
                return crossed.All(x => x.Metric.GetOrDefault(MacdValue) < 0 && x.Metric.GetOrDefault(MacdSignal) < 0);
            }
        }

        public static class Ema
        {   
            public static bool IsUpTrend(TradeQuote last)
            {
                return last.Metric.GetOrDefault(Ema200) < last.Close;
            }

            public static bool IsPositiveTrend(IList<TradeQuote> quotes, PeriodSize periodSize)
            {
                var periods = (int) (periodSize.ToObserverPeriod().TotalMinutes / periodSize.ToTimeSpan().TotalMinutes);
                var tradeQuote = quotes.TakeLast(periods+1).First();
                return quotes.Last().Metric.GetOrDefault(Ema200) > tradeQuote.Metric.GetOrDefault(Ema200);
            }
        }


        public static decimal GetPullBack(IReadOnlyCollection<TradeQuote> dataByMinute, decimal boughtAtPrice =-1, decimal minRisk = 0.01m,
            decimal maxRisk = 0.02m)
        {
            if (boughtAtPrice == -1) boughtAtPrice = dataByMinute.Last().Close;
            var highest = dataByMinute.Select((x, i) => new {i, x}).OrderByDescending(x => x.x.High).First();
            var lowest = dataByMinute.Skip(highest.i).OrderBy(x => x.Low).First();
            var leastAmountOfRisk = (1m - minRisk) * boughtAtPrice;
            var maxAmountOfRisk = (1m - maxRisk) * boughtAtPrice;
            var stopLost = Math.Min(leastAmountOfRisk, Math.Max(lowest.Low, maxAmountOfRisk));
            return stopLost;
        }

        public static bool IsPositiveTrend(IEnumerable<TradeQuote> values)
        {
            decimal lastValue = -1;
            foreach (var value in values)
            {
                if (lastValue != -1 && value.Close <= lastValue)
                {
                    return false;
                }

                lastValue = value.Close;
                
            }

            return true;
        }

        public static class Rsi
        {
            public static bool HasBuySignal(IEnumerable<TradeQuote> tradeQuotes, in int buySignal)
            {
                return MinRsi(tradeQuotes) <= buySignal;
            }

            public static decimal? MinRsi(IEnumerable<TradeQuote> tradeQuotes)
            {
                return tradeQuotes.Min(x => x.Metric.GetOrDefault("rsi14"));
            }
        }
    }
}