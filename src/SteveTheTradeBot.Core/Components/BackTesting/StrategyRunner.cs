using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class StrategyRunner : IStrategyRunner
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly StrategyPicker _strategyPicker;
        private readonly IDynamicGraphs _dynamicGraphs;
        private readonly IStrategyInstanceStore _strategyInstanceStore;
        private readonly ITradeHistoryStore _historyStore;
        private readonly IBrokerApi _broker;

        public StrategyRunner(StrategyPicker strategyPicker, IDynamicGraphs dynamicGraphs,
            IStrategyInstanceStore strategyInstanceStore, ITradeHistoryStore historyStore, IBrokerApi broker)
        {
            _strategyPicker = strategyPicker;
            _dynamicGraphs = dynamicGraphs;
            _strategyInstanceStore = strategyInstanceStore;
            _historyStore = historyStore;
            _broker = broker;
        }


        #region Implementation of IStrategyRunner

        public async Task Process(StrategyInstance strategyInstance, DateTime time)
        {
            if (strategyInstance.IsBackTest) throw new ArgumentException("Cannot process back test strategy!");
            if (!strategyInstance.IsActive)
                throw new ArgumentException("Cannot process strategy that is marked as inactive!");
            if (!IsCorrectTime(strategyInstance.PeriodSize, time)) return;
            var stopwatch = new Stopwatch().With(x => x.Start());
            try
            {
                var strategy = _strategyPicker.Get(strategyInstance.StrategyName);
                var strategyContext = await PopulateStrategyContext(strategyInstance, time);
                strategyContext.ByMinute.Dump("strategyContext.ByMinute");
                PreRun(strategyInstance, strategyContext.ByMinute.Last());
                await strategy.DataReceived(strategyContext);
                PostRun(strategyInstance, strategyContext.ByMinute.Last());
            }
            finally
            {
                await PostTransaction(strategyInstance);
                _log.Information($"Done processing {strategyInstance.Reference} in {stopwatch.Elapsed.ToShort()}");
            }
        }

        private static void PostRun(StrategyInstance strategyInstance, TradeFeedCandle last)
        {
            strategyInstance.LastClose = last.Close;
            strategyInstance.LastDate = last.Date;
        }

        private static void PreRun(StrategyInstance strategyInstance, TradeFeedCandle last)
        {
            if (strategyInstance.FirstClose != 0) return;
            strategyInstance.FirstClose = last.Close;
            strategyInstance.FirstStart = last.Date;
        }

        public async Task PostTransaction(StrategyInstance instance)
        {
            await _dynamicGraphs.Flush();
            instance.Recalculate();
            await _strategyInstanceStore.Update(instance);
        }

        private async Task<StrategyContext> PopulateStrategyContext(StrategyInstance strategyInstance, DateTime time)
        {
            var strategyContext = new StrategyContext(_dynamicGraphs, strategyInstance, _broker);
            var findRecentCandles =
                await _historyStore.FindRecentCandles(strategyInstance.PeriodSize, time, 500, strategyInstance.Feed);
            strategyContext.ByMinute.AddRange(findRecentCandles.OrderBy(x => x.Date));
            return strategyContext;
        }

        #endregion


        public bool IsCorrectTime(PeriodSize periodSize, DateTime dateTime1)
        {
            var dateTime = dateTime1.ToMinute();
            var timeSpan = periodSize.ToTimeSpan();
            if (timeSpan.TotalMinutes < 60)
            {
                return Math.Abs(dateTime.Minute % timeSpan.TotalMinutes) < 0.5;
            }

            if (timeSpan.TotalHours < 24)
            {
                return dateTime.Minute == 0 && Math.Abs(dateTime.Hour % timeSpan.TotalHours) < 0.5;
            }

            if (periodSize == PeriodSize.Day)
            {
                return dateTime.Hour == 0 && dateTime.Minute == 0 &&
                       Math.Abs(dateTime.DayOfYear % timeSpan.TotalDays) < 0.5;
            }

            if (periodSize == PeriodSize.Week)
            {
                return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.DayOfWeek == 0;
            }

            return false;
        }
    }
}