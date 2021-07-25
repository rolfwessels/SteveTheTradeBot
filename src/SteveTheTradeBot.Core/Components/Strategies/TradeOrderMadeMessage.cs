using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class TradeOrderMadeMessage
    {
        public StrategyInstance StrategyInstance { get; }
        public StrategyTrade StrategyTrade { get; }
        public TradeOrder Order { get; }

        public TradeOrderMadeMessage(StrategyInstance strategyInstance, StrategyTrade strategyTrade, TradeOrder order)
        {
            StrategyInstance = strategyInstance;
            StrategyTrade = strategyTrade;
            Order = order;
        }
    }
}