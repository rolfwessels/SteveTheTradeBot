using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class TestBuySellStrategy : BaseStrategy
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public const string Desc = "TestBuySell";

        #region Overrides of BaseStrategy

        public override async Task DataReceived(StrategyContext data)
        {
            var activeTrade = data.ActiveTrade();
            if (activeTrade == null)
            {
                var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                await SetStopLoss(data, strategyTrade.BuyPrice * 0.98m);
                data.StrategyInstance.IsActive = false;
            }
            else
            {
                await Sell(data, activeTrade);
                //_log.Information("------done setting to IsActive = false");
                data.StrategyInstance.IsActive = false;
            }
        }

        public override string Name { get; } = Desc;

        #endregion
    }
}