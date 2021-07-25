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
    public class PopulateOtherMetrics : BackgroundService
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ITradeFeedCandlesStore _store;
        private readonly IParameterStore _parameterStore;
        private readonly IMessenger _messenger;
        readonly ManualResetEventSlim _delayWorker = new ManualResetEventSlim(false);

        public PopulateOtherMetrics(ITradeFeedCandlesStore store, IParameterStore parameterStore, IMessenger messenger)
        {
            _store = store;
            _parameterStore = parameterStore;
            _messenger = messenger;
        }

        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            _messenger.Register<PopulateOneMinuteCandleService.Updated>(this, x => _delayWorker.Set());
            while (!token.IsCancellationRequested)
            {
                _delayWorker.Wait(token);
                foreach (var (period,feed) in ValrFeeds.AllWithPeriods())
                {
                    await Populate(token, feed.Name, feed.CurrencyPair, period);
                }

                await _messenger.Send(new Updated());
                await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
            }
        }

        public class Updated
        {
        }

        private async Task Populate(CancellationToken token, string feed, string currencyPair, PeriodSize periodSize)
        {
            var required = 400;
            var key = $"metric_populate_{feed}_{currencyPair}_{periodSize}";
            var startDate = await _parameterStore.Get(key, new DateTime(2000,1,1));
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

                    AddRsi(currentBatch, values);
                    GetRoc(currentBatch, values);
                    AddEmi(currentBatch, values);
                    AddGetMacd(currentBatch, values);
                    AddSuperTrend(currentBatch, values);
                    var keyValuePairs = values.Where(x=>x.Key.ToUniversalTime() >= startDate.AddHours(-1).ToUniversalTime()).ToList();
                    var updateFeed = await _store.UpdateFeed(keyValuePairs, feed, currencyPair, periodSize);
                    await _parameterStore.Set(key, batch.Last().Date);
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

        private static void AddSuperTrend(List<TradeFeedCandle> tradeFeedCandles, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            var rsiResults = tradeFeedCandles.GetSuperTrend();
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("supertrend-lower", rsiResult.LowerBand);
                orAdd.Add("supertrend-upper", rsiResult.UpperBand);
                orAdd.Add("supertrend", rsiResult.SuperTrend);
            }
        }

        private static void AddRsi(List<TradeFeedCandle> tradeFeedCandles, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            var rsiResults = tradeFeedCandles.GetRsi();
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("rsi14", rsiResult.Rsi);
            }
        }


        private static void GetRoc(List<TradeFeedCandle> tradeFeedCandles, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            var rsiResults = tradeFeedCandles.GetRoc(100, 100);
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("roc100", rsiResult.Roc);
                orAdd.Add("roc100-sma", rsiResult.RocSma);
            }
            rsiResults = tradeFeedCandles.GetRoc(200,200);
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("roc200", rsiResult.Roc);
                orAdd.Add("roc200-sma", rsiResult.RocSma);
            }
        }

        private static void AddGetMacd(List<TradeFeedCandle> tradeFeedCandles, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            var rsiResults = tradeFeedCandles.GetMacd();
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("macd-signal", rsiResult.Signal);
                orAdd.Add("macd-histogram", rsiResult.Histogram);
                orAdd.Add("macd", rsiResult.Macd);
            }
        }
        private static void AddEmi(List<TradeFeedCandle> tradeFeedCandles, IDictionary<DateTime, Dictionary<string, decimal?>> values)
        {
            var rsiResults = tradeFeedCandles.GetEma(100);
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("ema100", rsiResult.Ema);
            }
            rsiResults = tradeFeedCandles.GetEma(200);
            foreach (var rsiResult in rsiResults)
            {
                var orAdd = values.GetOrAdd(rsiResult.Date, () => new Dictionary<string, decimal?>());
                orAdd.Add("ema200", rsiResult.Ema);
            }
        }

        #endregion
    }

    
}