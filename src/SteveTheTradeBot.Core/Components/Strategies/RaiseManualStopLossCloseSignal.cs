using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RaiseManualStopLossCloseSignal : RaiseStopLossCloseSignal
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);


        public RaiseManualStopLossCloseSignal(decimal initialStopRisk = 0.96m, decimal moveProfitPercent = 1.05m) : base(initialStopRisk, moveProfitPercent)
        {
        }


        #region Overrides of RaiseStopLossCloseSignal

        public override async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade, BaseStrategy strategy)
        {
            await base.DetectClose(data, currentTrade, activeTrade, strategy);
            var updateStopLossAt = await data.Get(StrategyProperty.StopLoss, 0);
            if (currentTrade.Close <= updateStopLossAt)
            {
                _log.Information(
                    $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} - {activeTrade.BuyPrice} = {currentTrade.Close - activeTrade.BuyPrice} ");
                await strategy.Sell(data, activeTrade);
                data.StrategyInstance.Status = $"{activeTrade.ToString(data.StrategyInstance)}";
            }
        }

        #endregion

       

        #region Overrides of RaiseStopLossCloseSignal

        protected override Task SetTheStopLoss(StrategyContext data, BaseStrategy strategy, decimal lossAmount)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}