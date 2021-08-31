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
        private readonly ITradeQuoteStore _tradeQuoteStore;
        private readonly IBrokerApi _broker;
        private readonly IMessenger _messenger;
        private readonly IParameterStore _parameterStore;

        public StrategyRunner(StrategyPicker strategyPicker, IDynamicGraphs dynamicGraphs,
            IStrategyInstanceStore strategyInstanceStore, IBrokerApi broker, ITradeQuoteStore tradeQuoteStore, IMessenger messenger, IParameterStore parameterStore)
        {
            _strategyPicker = strategyPicker;
            _dynamicGraphs = dynamicGraphs;
            _strategyInstanceStore = strategyInstanceStore;
            _broker = broker;
            _tradeQuoteStore = tradeQuoteStore;
            _messenger = messenger;
            _parameterStore = parameterStore;
        }

        #region Implementation of IStrategyRunner

        public async Task<bool> Process(StrategyInstance strategyInstance, DateTime time)
        {
            if (strategyInstance.IsBackTest) throw new ArgumentException("Cannot process back test strategy!");
            if (!strategyInstance.IsActive)
                throw new ArgumentException("Cannot process strategy that is marked as inactive!");
            if (time < strategyInstance.LastDate.Add(strategyInstance.PeriodSize.ToTimeSpan())) return false;
            var stopwatch = new Stopwatch().With(x => x.Start());
            return await _strategyInstanceStore.EnsureUpdate(strategyInstance.Id, async (strategy) => {
                return await ProcessStrategy(strategy, time, stopwatch);
            });
            
        }

        private async Task<bool> ProcessStrategy(StrategyInstance instance, DateTime time, Stopwatch stopwatch)
        {
            try
            {
                var strategy = _strategyPicker.Get(instance.StrategyName);
                var strategyContext = await PopulateStrategyContext(instance, time);
                if (!ValidateInputDate(instance, time, strategyContext))
                {
                    return false;
                }
                await Process(instance, strategyContext, strategy);
                return true;
            }
            finally
            {
                await PostTransaction(instance);
                _log.Information($"Done processing {instance.Name} in {stopwatch.Elapsed.ToShort()}");
            }
        }

        public async Task Process(StrategyInstance instance, StrategyContext strategyContext, IStrategy strategy)
        {
            PreRun(instance, strategyContext.Quotes.Last());
            var syncOrderStatus = await strategyContext.Broker.SyncOrderStatus(instance, strategyContext);
            if (!syncOrderStatus)
            {
                await strategy.DataReceived(strategyContext);
            }
            PostRun(instance, strategyContext.Quotes.Last());
        }

        private static bool ValidateInputDate(StrategyInstance instance, DateTime time, StrategyContext strategyContext)
        {
            var lastQuoteDate = strategyContext.LatestQuote().Date.ToLocalTime();
            var instanceLastDate = instance.LastDate.ToLocalTime();
            if (lastQuoteDate < instanceLastDate)
            {
                _log.Debug(
                    $"[ERR] ! StrategyRunner {instance.StrategyName} {instance.Id} last run date is *{instanceLastDate}* and we are trying to run a old trade of {lastQuoteDate} ! !");
                return false;
            }

            if (lastQuoteDate == instanceLastDate)
            {
                var timeSinceLastRun = (DateTime.Now.ToLocalTime() - instanceLastDate);
                var warningTime = instance.PeriodSize.ToTimeSpan() * 10;
                if (timeSinceLastRun > warningTime)
                {
                    _log.Warning(
                        $"StrategyRunner {instance.StrategyName} {instance.Id} last run date is *{instanceLastDate}* and we are trying to run it again at {time.ToLocalTime()} that's {timeSinceLastRun.ToShort()} ago!");
                }
                else
                {
                    _log.Debug(
                        $"StrategyRunner {instance.StrategyName} {instance.Id} last run date is *{instanceLastDate}* and we are trying to run it again at {time.ToLocalTime()} that's {timeSinceLastRun.ToShort()} ago!");
                }
                return false;
            }

            return true;
        }

        public void PostRun(StrategyInstance strategyInstance, TradeQuote last)
        {
            strategyInstance.LastClose = last.Close;
            strategyInstance.LastDate = last.Date;
        }

        public void PreRun(StrategyInstance strategyInstance, TradeQuote last)
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

        public async Task<StrategyContext> PopulateStrategyContext(StrategyInstance strategyInstance, DateTime time)
        {
            var strategyContext = new StrategyContext(_dynamicGraphs, strategyInstance, _broker, _messenger,_parameterStore);
            var findRecentQuotes =
                await _tradeQuoteStore.FindRecent(strategyInstance.PeriodSize, time.ToUniversalTime(), 500, strategyInstance.Pair, strategyInstance.Feed);

            var findDayQuotes =
                await _tradeQuoteStore.FindRecent(PeriodSize.Day, time.ToUniversalTime(), 60, strategyInstance.Pair, strategyInstance.Feed);

            var tradeFeedQuotes = findRecentQuotes
                    .Where(x=> x.Metric != null && x.Metric.Count > 1)
                    .OrderBy(x => x.Date);
            strategyContext.Quotes.AddRange(tradeFeedQuotes);
            strategyContext.DayQuotes.AddRange(findDayQuotes);

            if (strategyContext.Quotes.Count < 100) throw new Exception("Missing Quotes data!");
            _log.Debug($"StrategyRunner:PopulateStrategyContext Look for trades before: {time.ToUniversalTime()} and found {strategyContext.LatestQuote().Date}!");
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