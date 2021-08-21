using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RaiseManualStopLossCloseSignal : ICloseSignal
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        protected decimal _initialStopRisk;
        protected decimal _moveProfitPercent;

        public RaiseManualStopLossCloseSignal(decimal initialStopRisk= 0.96m, decimal moveProfitPercent = 1.05m)
        {
            _initialStopRisk = initialStopRisk;
            _moveProfitPercent = moveProfitPercent;
        }
        
        public async Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice,
            BaseStrategy strategy)
        {
            return await ResetStops(data, boughtAtPrice);
        }

        public async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade, BaseStrategy strategy)
        {
            var updateStopLossAt = await UpdateStopLossAt(data);
            var stopLoss = await StopLoss(data);
            if (currentTrade.Close > updateStopLossAt)
            {
                var oldStopLoss = stopLoss;
                var newStopLoss = await ResetStops(data, currentTrade.Close);
                data.StrategyInstance.Status = $"Update stop loss to {newStopLoss} that means guaranteed profit of {TradeUtils.MovementPercent(newStopLoss, activeTrade.BuyValue)}%";
                await data.Messenger.Send(PostSlackMessage.From($"{data.StrategyInstance.Name} {data.StrategyInstance.Status} :chart_with_upwards_trend:"));
            }
            else if (currentTrade.Close <= stopLoss)
            {
                _log.Information(
                    $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} - {activeTrade.BuyPrice} = {currentTrade.Close - activeTrade.BuyPrice} ");
                await strategy.Sell(data, activeTrade);
                data.StrategyInstance.Status = $"Sold! {activeTrade}";
            }
            else
            {
                data.StrategyInstance.Status = $"Waiting for price above {updateStopLossAt} or stop loss {stopLoss}";
            }
        }

        private async Task<decimal?> UpdateStopLossAt(StrategyContext data, decimal? setValue = null)
        {
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _moveProfitPercent;
                return await data.Get(StrategyProperty.UpdateStopLossAt, moveProfitPercent);
            }
            await data.Set(StrategyProperty.UpdateStopLossAt, setValue.Value);
            return setValue;
        }

        protected async Task<decimal?> StopLoss(StrategyContext data, decimal? setValue = null)
        {
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _initialStopRisk;
                return await data.Get(StrategyProperty.StopLoss, moveProfitPercent);
            }
            await data.Set(StrategyProperty.StopLoss, setValue.Value);
            return setValue;
        }

        protected async Task<decimal> ResetStops(StrategyContext data, decimal currentTradeClose)
        {
            await StopLoss(data, currentTradeClose * _initialStopRisk);
            await UpdateStopLossAt(data, currentTradeClose * _moveProfitPercent);
            return currentTradeClose * _initialStopRisk;
        }
    }
}