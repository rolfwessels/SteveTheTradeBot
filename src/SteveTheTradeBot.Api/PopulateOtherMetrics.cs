using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using GreenDonut;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class PopulateOtherMetrics : BackgroundServiceWithResetAndRetry
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ITradeQuoteStore _store;
        private readonly IParameterStore _parameterStore;
        private readonly IMessenger _messenger;

        public PopulateOtherMetrics(ITradeQuoteStore store, IParameterStore parameterStore, IMessenger messenger)
        {
            _store = store;
            _parameterStore = parameterStore;
            _messenger = messenger;
        }

        #region Overrides of BackgroundService

        protected override void RegisterSetter()
        {
            _messenger.Register<PopulateOtherQuotesService.UpdatedOtherQuotes>(this, x => _delayWorker.Set());
        }

        protected override async Task ExecuteAsyncInRetry(CancellationToken token)
        {
            foreach (var (period, feed) in ValrFeeds.AllWithPeriods())
            {
                await Populate(token, feed.Name, feed.CurrencyPair, period);
            }

            await _messenger.Send(new MetricsUpdatedMessage());
        }

        public class MetricsUpdatedMessage
        {
        }

        private async Task Populate(CancellationToken token, string feed, string currencyPair, PeriodSize periodSize)
        {
            var required = 500;
            var key = $"metric_populate_{feed}_{currencyPair}_{periodSize}";
            var startDate = (await _parameterStore.Get(key, new DateTime(2000,1,1))).ToUniversalTime();
            try
            {
                var findAllBetween = _store.FindAllBetween(startDate, DateTime.Now.ToUniversalTime(), feed, currencyPair, periodSize);
                var prevBatch = await _store.FindBefore(startDate, feed, currencyPair, periodSize, required);
                foreach (var batch in findAllBetween.BatchedBy(required*2))
                {
                    var values = new Dictionary<DateTime,Dictionary<string,decimal?>>();
                    if (token.IsCancellationRequested) return;
                    var currentBatch = prevBatch.Concat(batch).OrderBy(x=>x.Date).ToList();
                    if (currentBatch.Count < required)
                    {
                        _log.Debug($"Skip processing  {feed}, currencyPair {currencyPair} , periodSize {periodSize} because we only have {currentBatch.Count} historical items.");
                        break;
                    }

                    AddSuperTrend(currentBatch, values);
                    AddGetMacd(currentBatch, values);
                    AddEmi(currentBatch, values);
                    AddRoc(currentBatch, values);
                    AddRsi(currentBatch, values);
                    var keyValuePairs = values.Where(x=>x.Key.ToUniversalTime() >= startDate.Add(periodSize.ToTimeSpan()*-1)).ToList();
                    var updateFeed = await _store.UpdateFeed(keyValuePairs, feed, currencyPair, periodSize);
                    await _parameterStore.Set(key, keyValuePairs.Last().Key);
                    startDate = batch.Last().Date;
                    prevBatch = currentBatch.TakeLast(required).ToList();
                    _log.Debug($"PopulateOtherMetrics:Populate {key} with {updateFeed.Count} / {keyValuePairs.Count} entries before LastDate:{startDate} {currentBatch.Count} records used from {currentBatch.Min(x=>x.Date)} to {currentBatch.Max(x => x.Date)} {values.Keys.Min()} {values.Keys.Max()}.");
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                _log.Warning($"PopulateOtherMetrics:Populate {key} {e.Message}");
            }
        }

        private static void AddSuperTrend(List<TradeQuote> tradeFeedQuotes, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            if (tradeFeedQuotes.Count < 250) return;
            var rsiResults = tradeFeedQuotes.GetSuperTrend();
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("supertrend-lower", rsiResult.LowerBand);
                orAdd.Add("supertrend-upper", rsiResult.UpperBand);
                orAdd.Add("supertrend", rsiResult.SuperTrend);
            }
        }

        private static void AddRsi(List<TradeQuote> tradeFeedQuotes, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            if (tradeFeedQuotes.Count < 140) return;
            var rsiResults = tradeFeedQuotes.GetRsi();
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("rsi14", rsiResult.Rsi);
            }
        }


        private static void AddRoc(List<TradeQuote> tradeFeedQuotes, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            if (tradeFeedQuotes.Count < 101) return;
            var roc = tradeFeedQuotes.GetRoc(100, 100);
            foreach (var rsiResult in roc)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("roc100", rsiResult.Roc);
                orAdd.Add("roc100-sma", rsiResult.RocSma);
            }

            if (tradeFeedQuotes.Count < 202) return;
            roc = tradeFeedQuotes.GetRoc(200,200).ToList();
            if (roc.TakeLast(30).Any(x => x.RocSma == null))
            {
                foreach (var rsi in roc)
                {
                    _log.Debug($"PopulateOtherMetrics:AddRoc Failing to get values for {rsi.Date} - {rsi.RocSma}");
                }
                throw new Exception("Fail!");
            }

            foreach (var rsiResult in roc)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("roc200", rsiResult.Roc);
                orAdd.Add("roc200-sma", rsiResult.RocSma);
            }
        }

        private static void AddGetMacd(List<TradeQuote> tradeFeedQuotes, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            if (tradeFeedQuotes.Count < 135) return;
            var rsiResults = tradeFeedQuotes.GetMacd();
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("macd-signal", rsiResult.Signal);
                orAdd.Add("macd-histogram", rsiResult.Histogram);
                orAdd.Add("macd", rsiResult.Macd);
            }
        }
        private static void AddEmi(List<TradeQuote> tradeFeedQuotes, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            if (tradeFeedQuotes.Count < 200) return;
            var rsiResults = tradeFeedQuotes.GetEma(100);
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("ema100", rsiResult.Ema);
            }
            if (tradeFeedQuotes.Count < 400) return;
            rsiResults = tradeFeedQuotes.GetEma(200);
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("ema200", rsiResult.Ema);
            }
        }

        #endregion
    }

    
}