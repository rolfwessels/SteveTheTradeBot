using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class PopulateOneMinuteCandleService : BackgroundService
    {
        private readonly ITradePersistenceFactory _factory;
        private readonly IHistoricalDataPlayer _historicalDataPlayer;

        public PopulateOneMinuteCandleService(ITradePersistenceFactory factory, IHistoricalDataPlayer historicalDataPlayer)
        {
            _factory = factory;
            _historicalDataPlayer = historicalDataPlayer;
        }

        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var valrFeed in ValrFeeds.All)
                {
                    await Populate(token, valrFeed.CurrencyPair, valrFeed.Name);
                }
                await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
            }
        }

        public async Task Populate(CancellationToken token, string currencyPair, string feed)
        {
            
            var periodSize = PeriodSize.OneMinute;
            var from = DateTime.Now.AddYears(-10);
            var context = await _factory.GetTradePersistence();
            var foundCandle = context.TradeFeedCandles.AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize)
                .OrderByDescending(x => x.Date).Take(1).FirstOrDefault();
            if (foundCandle != null)
            {
                from = foundCandle.Date;
                context.Remove(foundCandle);
                context.SaveChanges();
            }
            var stopwatch = new Stopwatch().With(x => x.Start());
            var readHistoricalTrades = _historicalDataPlayer.ReadHistoricalTrades(currencyPair, from, DateTime.Now, token);
            var candles = readHistoricalTrades.ToCandleOneMinute().Select(x => TradeFeedCandle.From(x, feed, periodSize, currencyPair));
            
            foreach (var feedCandles in candles.BatchedBy())
            {
                await using var saveContext = await _factory.GetTradePersistence();
                if (token.IsCancellationRequested) return;
                saveContext.TradeFeedCandles.AddRange(feedCandles);
                var count = await saveContext.SaveChangesAsync(token);
                _log.Information($"Saved {count} {periodSize} candles for {currencyPair} in {stopwatch.Elapsed.ToShort()}.");
                stopwatch.Restart();
            }
        }

        #endregion
    }
}