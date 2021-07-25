using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class PopulateOneMinuteCandleService : BackgroundService
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ITradePersistenceFactory _factory;
        private readonly IHistoricalDataPlayer _historicalDataPlayer;
        private readonly IMessenger _messenger;
        readonly ManualResetEventSlim _delayWorker = new ManualResetEventSlim(false);

        public PopulateOneMinuteCandleService(ITradePersistenceFactory factory,
            IHistoricalDataPlayer historicalDataPlayer, IMessenger messenger)
        {
            _factory = factory;
            _historicalDataPlayer = historicalDataPlayer;
            _messenger = messenger;
        }

        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            _messenger.Register<TickerTrackerService.UpdatedMessage>(this, x => _delayWorker.Set());
            while (!token.IsCancellationRequested)
            {
                _delayWorker.Wait(token);

                var tasks = ValrFeeds.All.Select(valrFeed => Populate(token, valrFeed.CurrencyPair, valrFeed.Name)).ToList();
                await Task.WhenAll(tasks);

                await _messenger.Send(new Updated(tasks.Select(x=>x.Result).ToList()));
                await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
            }
        }

        public async Task<TradeFeedCandle> Populate(CancellationToken token, string currencyPair, string feed)
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
            }

            var stopwatch = new Stopwatch().With(x => x.Start());
            var readHistoricalTrades =
                _historicalDataPlayer.ReadHistoricalTrades(currencyPair, from, DateTime.Now, token);
            var candles = readHistoricalTrades.ToCandleOneMinute()
                .Select(x => TradeFeedCandle.From(x, feed, periodSize, currencyPair));
            TradeFeedCandle lastCandle = null;
            foreach (var feedCandles in candles.BatchedBy())
            {
                
                if (token.IsCancellationRequested) return null;
                foreach (var cdl in feedCandles)
                {
                    if (cdl.Date == foundCandle?.Date)
                    {   
                        context.TradeFeedCandles.Update(cdl.CopyValuesTo(foundCandle));
                    }
                    else
                    {
                        context.TradeFeedCandles.Add(cdl);
                    }
                }
                
                var count = await context.SaveChangesAsync(token);
                _log.Information(
                    $"Saved {count} {periodSize} candles for {currencyPair} in {stopwatch.Elapsed.ToShort()}.");
                stopwatch.Restart();
                
                lastCandle = feedCandles.Last();
            }
            return lastCandle;
        }

        #endregion

        public class Updated
        {
            public List<TradeFeedCandle> Candles { get; }

            public Updated(List<TradeFeedCandle> candles)
            {
                Candles = candles;
            }
        }
    }

    
}