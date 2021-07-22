using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public static bool IsFirstRunDone { get; set; }

        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!PopulateOneMinuteCandleService.IsFirstRunDone)
                {
                    _log.Debug($"PopulateOtherCandlesService: Waiting for feed to be populated.");
                    await Task.Delay(TimeSpan.FromSeconds(2), token);
                    continue;
                }
                var tasks = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.OneMinute)
                    .Select(x =>
                    {
                        var (periodSize, feed) = x;
                        return Populate(token, feed.CurrencyPair, feed.Name, periodSize);
                    });
                await Task.WhenAll(tasks);
                IsFirstRunDone = true;
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
            var tradeFeedCandles = _store.FindAllBetween(@from, DateTime.Now, feed, currencyPair, PeriodSize.OneMinute);
            var candles = tradeFeedCandles.Aggregate(periodSize)
                .Select(x => TradeFeedCandle.From(x, feed, periodSize, currencyPair));

            foreach (var feedCandles in candles.BatchedBy())
            {
                if (token.IsCancellationRequested) return;
                var count = await _store.AddRange(feedCandles);
                _log.Information(
                    $"Saved {count} {periodSize} candles for {currencyPair} in {stopwatch.Elapsed.ToShort()}.");
                stopwatch.Restart();
            }
        }

        #endregion
    }
}