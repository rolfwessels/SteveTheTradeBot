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
using SteveTheTradeBot.Core.Framework.MessageUtil;
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
        private readonly ITradeFeedCandlesStore _tradeFeedCandleStore;
        private readonly IBrokerApi _broker;
        private readonly IMessenger _messenger;

        public StrategyRunner(StrategyPicker strategyPicker, IDynamicGraphs dynamicGraphs,
            IStrategyInstanceStore strategyInstanceStore, IBrokerApi broker, ITradeFeedCandlesStore tradeFeedCandleStore, IMessenger messenger)
        {
            _strategyPicker = strategyPicker;
            _dynamicGraphs = dynamicGraphs;
            _strategyInstanceStore = strategyInstanceStore;
            _broker = broker;
            _tradeFeedCandleStore = tradeFeedCandleStore;
            _messenger = messenger;
        }


        #region Implementation of IStrategyRunner

        public async Task Process(StrategyInstance strategyInstance, DateTime time)
        {
            if (strategyInstance.IsBackTest) throw new ArgumentException("Cannot process back test strategy!");
            if (!strategyInstance.IsActive)
                throw new ArgumentException("Cannot process strategy that is marked as inactive!");
            if (!IsCorrectTime(strategyInstance.PeriodSize, time)) return;
            var stopwatch = new Stopwatch().With(x => x.Start());
            await _strategyInstanceStore.EnsureUpdate(strategyInstance.Id, async (strategy) => {
                await ProcessStrategy(strategy, time, stopwatch);
                return true;
            });
            
        }

        private async Task ProcessStrategy(StrategyInstance instance, DateTime time, Stopwatch stopwatch)
        {
            try
            {
                var strategy = _strategyPicker.Get(instance.StrategyName);
                var strategyContext = await PopulateStrategyContext(instance, time);
                PreRun(instance, strategyContext.ByMinute.Last());
                await strategy.DataReceived(strategyContext);
                PostRun(instance, strategyContext.ByMinute.Last());
            }
            finally
            {
                await PostTransaction(instance);
                _log.Information($"Done processing {instance.Reference} in {stopwatch.Elapsed.ToShort()}");
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
        }

        private async Task<StrategyContext> PopulateStrategyContext(StrategyInstance strategyInstance, DateTime time)
        {
            var strategyContext = new StrategyContext(_dynamicGraphs, strategyInstance, _broker, _messenger);
            var findRecentCandles =
                await _tradeFeedCandleStore.FindRecentCandles(strategyInstance.PeriodSize, time, 500, strategyInstance.Pair, strategyInstance.Feed);
            strategyContext.ByMinute.AddRange(findRecentCandles.OrderBy(x => x.Date));
            if (strategyContext.ByMinute.Count < 100) throw new Exception("Missing ByMinute data!");
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