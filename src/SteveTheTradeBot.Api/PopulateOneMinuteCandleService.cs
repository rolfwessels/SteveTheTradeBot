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
    public class PopulateOneMinuteCandleService : BackgroundServiceWithResetAndRetry
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ITradePersistenceFactory _factory;
        private readonly IHistoricalDataPlayer _historicalDataPlayer;
        private readonly IMessenger _messenger;

        public PopulateOneMinuteCandleService(ITradePersistenceFactory factory,
            IHistoricalDataPlayer historicalDataPlayer, IMessenger messenger)
        {
            _factory = factory;
            _historicalDataPlayer = historicalDataPlayer;
            _messenger = messenger;
        }

        #region Overrides of BackgroundService

        protected override void RegisterSetter()
        {
            _messenger.Register<TickerTrackerService.TickerUpdatedMessage>(this, x => _delayWorker.Set());
        }

        protected override async Task ExecuteAsyncInRetry(CancellationToken token)
        {
            var tasks = ValrFeeds.All.Select(valrFeed => Populate(token, valrFeed.CurrencyPair, valrFeed.Name)).ToList();
            await Task.WhenAll(tasks);
            await _messenger.Send(new OneMinuteCandleAvailable(tasks.Select(x => x.Result).ToList()));
        }

        public async Task<TradeQuote> Populate(CancellationToken token, string currencyPair, string feed)
        {
            var periodSize = PeriodSize.OneMinute;
            var from = DateTime.Now.AddYears(-10);
            var context = await _factory.GetTradePersistence();
            var foundCandle = context.TradeQuotes.AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize)
                .OrderByDescending(x => x.Date).Take(1).FirstOrDefault();
            if (foundCandle != null)
            {
                from = foundCandle.Date;
            }

            var stopwatch = new Stopwatch().With(x => x.Start());
            var readHistoricalTrades =
                _historicalDataPlayer.ReadHistoricalTrades(currencyPair, from.ToUniversalTime(), DateTime.UtcNow, token);
            var lastTrade = DateTime.Now.AddYears(-10);
            var candles = readHistoricalTrades
                .ForAll(x=>lastTrade = x.TradedAt)
                .ToCandleOneMinute()
                .Select(x => TradeQuote.From(x, feed, periodSize, currencyPair));
            TradeQuote lastCandle = null;
            foreach (var feedCandles in candles.BatchedBy())
            {
                
                if (token.IsCancellationRequested) return null;
                foreach (var cdl in feedCandles)
                {
                    if (cdl.Date == foundCandle?.Date)
                    {   
                        context.TradeQuotes.Update(cdl.CopyValuesTo(foundCandle));
                    }
                    else
                    {
                        context.TradeQuotes.Add(cdl);
                    }
                }
                
                var count = await context.SaveChangesAsync(token);
                lastCandle = feedCandles.OrderBy(x=>x.Date).Last();
                _log.Information(
                    $"Saved {count} {periodSize} candles for {currencyPair} found candle {from} [{(DateTime.UtcNow - from).ToShort()}] saved {lastCandle.Date} [{(DateTime.UtcNow - lastCandle.Date).ToShort()}] [LT:{lastTrade}] in {stopwatch.Elapsed.ToShort()}.");
                stopwatch.Restart();
                
                
            }
            return lastCandle;
        }

        #endregion

        public class OneMinuteCandleAvailable
        {
            public List<TradeQuote> Candles { get; }

            public OneMinuteCandleAvailable(List<TradeQuote> candles)
            {
                Candles = candles;
            }
        }
    }

    
}