using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public interface ICloseSignal
    {
        Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice, BaseStrategy strategy);
        Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade, BaseStrategy strategy);
    }
}