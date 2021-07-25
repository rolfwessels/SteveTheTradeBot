using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class TestBuySellStrategy : BaseStrategy
    {
        public const string Desc = "TestBuySell";

        #region Overrides of BaseStrategy

        public override async Task DataReceived(StrategyContext data)
        {
            var activeTrade = data.ActiveTrade();
            if (activeTrade == null)
            {
                await Buy(data, data.StrategyInstance.BaseAmount);
            }
            else
            {
                await Sell(data, activeTrade);
            }
        }

        public override string Name { get; } = Desc;

        #endregion
    }
}