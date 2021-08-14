using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tools;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class StrategyContext : IParamsStoreSimple
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDynamicGraphs _dynamicGraphs;
        private readonly IParameterStore _parameterStore;

        public StrategyContext(IDynamicGraphs dynamicGraphs, StrategyInstance strategyInstance, IBrokerApi broker, IMessenger messenger, IParameterStore parameterStore)
        {
            _dynamicGraphs = dynamicGraphs;
            StrategyInstance = strategyInstance;
            _parameterStore = parameterStore;
            Messenger = messenger;
            ByMinute = new Recent<TradeQuote>(1000);
            Broker = broker;
        }

        public Recent<TradeQuote> ByMinute { get; }
        public StrategyInstance StrategyInstance { get; }

        public IBrokerApi Broker { get; }
        public IMessenger Messenger { get; }
        

        public Task Set(string key, string value)
        {
            StrategyInstance.Set(key, value);
            return Task.CompletedTask;
        }

        public Task<string> Get(string key, string defaultValue)
        {
            return Task.FromResult(StrategyInstance.Get(key, defaultValue));
        }

        public async Task PlotRunData(DateTime date, string label, decimal value)
        {
            _log.Debug($"StrategyContext:PlotRunData {StrategyInstance.Reference} {date}, {label}, {value}");
            await _dynamicGraphs.Plot(StrategyInstance.Reference, date, label, value);
        }

        public TradeQuote LatestQuote()
        {
            return ByMinute.Last();
        }

        public StrategyTrade ActiveTrade()
        {
            return StrategyInstance.Trades.FirstOrDefault(x => x.IsActive);
        }
    }
}