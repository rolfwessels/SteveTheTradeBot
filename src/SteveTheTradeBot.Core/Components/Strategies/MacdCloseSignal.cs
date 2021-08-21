using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class MacdCloseSignal : DynamicStopLossAndProfitCloseSignal
    {
        private string _useMacd;

        #region Implementation of ICloseSignal

        #region Overrides of DynamicStopLossAndProfitCloseSignal

        public override async Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice, BaseStrategy strategy)
        {
            var initialize = await base.Initialize(data, boughtAtPrice, strategy);
            _useMacd = "UseMacd";
            await data.Set(_useMacd, false);
            return initialize;
        }

        #endregion

        public override async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade, BaseStrategy strategy)
        {
            var useMacd = await data.Get(_useMacd, false);
            if (useMacd)
            {
                var isMacdSignalCrossingBack = Signals.Macd.GetCrossedSignalOverMacd(data.ByMinute.TakeLast(2)).Any();
                var isUpTrend = Signals.Ema.IsUpTrend(data.ByMinute.Last());
                
                if (isMacdSignalCrossingBack && !isUpTrend)
                {
                    await strategy.Sell(data, activeTrade);
                }
            }
            else
            {
                var changeStopLossAt = await data.Get(StrategyProperty.UpdateStopLossAt, 0m);
                if (currentTrade.Close > changeStopLossAt)
                {
                    var newStopLoss = activeTrade.BuyPrice * 1.001m;
                    await strategy.SetStopLoss(data, newStopLoss);
                    await data.Set(StrategyProperty.StopLoss, newStopLoss);
                    await data.Set(_useMacd, true);
                }
            }
        }

        #endregion
    }

    
}