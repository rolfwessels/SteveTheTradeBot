using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class DynamicStopLossAndProfitCloseSignal : ICloseSignal 
    {
        public DynamicStopLossAndProfitCloseSignal(int timesTheRisk = 3, decimal minRisk = 0.01m, decimal maxRisk = 0.02m)
        {
            MaxRisk = maxRisk;
            MinRisk = minRisk;
            TimesTheRisk = timesTheRisk;
        }

        public decimal TimesTheRisk { get;  }
        public decimal MaxRisk { get; }
        public decimal MinRisk { get; }

        #region Implementation of ICloseSignal

        public virtual async Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice, BaseStrategy strategy)
        {
            var stopLost = Signals.GetPullBack(data.Quotes, boughtAtPrice, MinRisk, MaxRisk);
            await strategy.SetStopLoss(data, stopLost);
            var risk = TradeUtils.MovementPercent(boughtAtPrice, stopLost)/100;
            await data.Set(StrategyProperty.BoughtAtPrice, boughtAtPrice);
            await data.Set(StrategyProperty.StopLoss,stopLost);
            await data.Set(StrategyProperty.Risk, risk);
            await data.Set(StrategyProperty.UpdateStopLossAt, (1+ risk) * boughtAtPrice);
            await data.Set(StrategyProperty.ExitAt, (1 + risk*TimesTheRisk) * boughtAtPrice);
            return stopLost;
        }

        public virtual async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade,
            BaseStrategy strategy)
        {
            if (currentTrade.Close > await data.Get(StrategyProperty.ExitAt, 0))
            {
                await strategy.Sell(data, activeTrade);
            }
            else if (currentTrade.Close > await data.Get(StrategyProperty.UpdateStopLossAt, 0))
            {
                var boughtAtPrice = await data.Get(StrategyProperty.BoughtAtPrice, 0);
                await strategy.SetStopLoss(data, boughtAtPrice);
            }

        }

        #endregion
    }
}