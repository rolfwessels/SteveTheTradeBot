using System.Collections.Generic;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public static class ValrFeeds
    {
        public static List<Feed> All => new List<Feed>() { new Feed(CurrencyPair.BTCZAR, "valr"), new Feed(CurrencyPair.ETHZAR, "valr") };

        public class Feed
        {
            public string CurrencyPair { get; }
            public string Name { get;  }
            public PeriodSize[] PeriodSizes { get;  }

            public Feed(string currencyPair, string valr)
            {
                CurrencyPair = currencyPair;
                Name = valr;
                PeriodSizes = new[]
                {
                    PeriodSize.OneMinute,
                    PeriodSize.FiveMinutes,
                    PeriodSize.FifteenMinutes,
                    PeriodSize.ThirtyMinutes,
                    PeriodSize.OneHour,
                    PeriodSize.Day,
                    PeriodSize.Week,
                    // PeriodSize.Month
                };
            }
        }

        public static IEnumerable<(PeriodSize,Feed)> AllWithPeriods()
        {
            foreach (var feed in All)
            {
                foreach (var feedPeriodSiz in feed.PeriodSizes)
                {
                    yield return (feedPeriodSiz, feed);
                }
            }
        }
    }

    
}