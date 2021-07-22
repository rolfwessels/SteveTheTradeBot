using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using GreenDonut;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class PopulateOtherMetrics : BackgroundService
    {
        public static bool IsFirstRunDone { get; private set; }
        private readonly ITradeFeedCandlesStore _store;
        private readonly IParameterStore _parameterStore;

        public PopulateOtherMetrics(ITradeFeedCandlesStore store, IParameterStore parameterStore)
        {
            _store = store;
            _parameterStore = parameterStore;
        }

        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!PopulateOtherCandlesService.IsFirstRunDone)
                {
                    _log.Debug("PopulateOtherMetrics: Waiting for feed to be populated.");
                    await Task.Delay(TimeSpan.FromSeconds(2), token);
                    continue;
                }
                foreach (var (period,feed) in ValrFeeds.AllWithPeriods())
                {
                    await Populate(token, feed.Name, feed.CurrencyPair, period);
                }
                IsFirstRunDone = true;
                await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().AddSeconds(1).TimeTill(), token);
            }
        }

        private async Task Populate(CancellationToken token, string feed, string currencyPair, PeriodSize periodSize)
        {
            var statName = "rsi14";
            var statName2 = "ema200";
            var required = 400;
            var key = $"metric_populate_{statName}_{feed}_{currencyPair}_{periodSize}";

            _log.Information($"Populate metrics for feed {feed}, currencyPair {currencyPair} , periodSize {periodSize}");
            var startDate = await _parameterStore.Get(key, DateTime.MinValue); 
            try
            {
                var findAllBetween = _store.FindAllBetween(startDate.Add(periodSize.ToTimeSpan()*required), DateTime.Now, feed, currencyPair, periodSize);
                List<TradeFeedCandle> prevBatch = new List<TradeFeedCandle>();
                foreach (var batch in findAllBetween.BatchedBy(required))
                {
                    var values = new Dictionary<DateTime,Dictionary<string,decimal?>>();
                    if (token.IsCancellationRequested) return;
                    var tradeFeedCandles = prevBatch.Concat(batch).ToList();
                    AddRsi(tradeFeedCandles, values);
                    GetRoc(tradeFeedCandles, values);
                    AddEmi(tradeFeedCandles, values);
                    AddGetMacd(tradeFeedCandles, values);
                    AddSuperTrend(tradeFeedCandles, values);
                    var fromDate = startDate;
                    var updateFeed = await _store.UpdateFeed(values.Where(x=>x.Key >= fromDate), feed, currencyPair, periodSize, statName);
                    await _parameterStore.Set(key, batch.Last().Date);
                    
                    startDate = batch.Last().Date;
                    prevBatch = batch;
                    _log.Debug($"PopulateOtherMetrics:Populate {key} with {updateFeed} entries LastDate:{startDate}");
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                _log.Warning($"PopulateOtherMetrics:Populate {e.Message}");
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