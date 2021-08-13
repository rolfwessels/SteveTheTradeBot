using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AutoMapper.Internal;
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
            public static List<TradeQuote> GetCrossed(IEnumerable<TradeQuote> takeLast)
            {
                var tradeQuotes = takeLast.ToList();
                var result = new List<TradeQuote>();
                for (int i = 1; i < tradeQuotes.Count(); i++)
                {
                    var prev = tradeQuotes[i - 1];
                    var current = tradeQuotes[i ];
                    if (prev.Metric.GetOrDefault(MacdValue) <= prev.Metric.GetOrDefault(MacdSignal))
                    {
                        if (current.Metric.GetOrDefault(MacdValue) > current.Metric.GetOrDefault(MacdSignal))
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
        }

        
    }
}