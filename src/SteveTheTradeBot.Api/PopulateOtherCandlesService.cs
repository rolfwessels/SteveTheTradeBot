using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Sinks.File;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class PopulateOtherCandlesService : BackgroundService
    {
        private readonly ITradeFeedCandlesStore _store;

        public PopulateOtherCandlesService(ITradeFeedCandlesStore store)
        {
            _store = store;
        }

        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var periodSizes = new[] {
                PeriodSize.FiveMinutes,
                PeriodSize.FifteenMinutes,
                PeriodSize.ThirtyMinutes,
                PeriodSize.OneHour,
                PeriodSize.Day,
                PeriodSize.Week,
                // PeriodSize.Month
                };
            while (!token.IsCancellationRequested)
            {
                
                foreach (var valrFeed in ValrFeeds.All)
                {
                    foreach (var period in periodSizes)
                    {
                        await Populate(token, valrFeed.CurrencyPair, valrFeed.Name , period);
                    }
                    
                }
                await Task.Delay(DateTime.Now.AddMinutes(5).ToMinute().TimeTill(), token);
            }
        }

        public async Task Populate(CancellationToken token, string currencyPair, string feed, PeriodSize periodSize)
        {
            var from = DateTime.Now.AddYears(-10);
            var foundCandle = await _store.FindLatestCandle(feed, currencyPair, periodSize);
            if (foundCandle != null)
            {
                from = foundCandle.Date;
                await _store.Remove(foundCandle);
            }
            var stopwatch = new Stopwatch().With(x => x.Start());
            var tradeFeedCandles = _store.FindAllBetween(@from,DateTime.Now, feed, currencyPair, PeriodSize.OneMinute);
            var candles = tradeFeedCandles.Aggregate(periodSize).Select(x => TradeFeedCandle.From(x, feed, periodSize, currencyPair));

            foreach (var feedCandles in candles.BatchedBy())
            {
                if (token.IsCancellationRequested) return;
                var count = await _store.AddRange(feedCandles);
                _log.Information($"Saved {count} {periodSize} candles for {currencyPair} in {stopwatch.Elapsed.ToShort()}.");
                stopwatch.Restart();
            }
        }

        #endregion
    }
}