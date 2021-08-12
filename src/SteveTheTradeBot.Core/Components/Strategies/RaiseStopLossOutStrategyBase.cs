using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public abstract class RaiseStopLossOutStrategyBase : BaseStrategy
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        protected decimal _initialStopRisk;
        protected decimal _moveProfitPercent;

        protected RaiseStopLossOutStrategyBase(decimal initialStopRisk, decimal moveProfitPercent)
        {
            _initialStopRisk = initialStopRisk;
            _moveProfitPercent = moveProfitPercent;
        }

        protected async Task<decimal> SetFirstStopLossFromPrice(StrategyContext data, decimal strategyTradeBuyPrice)
        {
            return await ResetStops(data, strategyTradeBuyPrice);
        }

        protected async Task FollowClosingStrategy(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade)
        {
            if (currentTrade.Close > await MoveProfit(data))
            {
                var oldStopLoss = await StopLoss(data);
                var newStopLoss = await ResetStops(data, currentTrade.Close);
                data.StrategyInstance.Status = $"Update stop loss to {newStopLoss} by {TradeUtils.MovementPercent(newStopLoss, oldStopLoss.GetValueOrDefault())}%";
                await data.Messenger.Send(
                    $"{data.StrategyInstance.Name} {data.StrategyInstance.Status} :chart_with_upwards_trend:");
            }
            else if (currentTrade.Close <= await StopLoss(data))
            {
                _log.Information(
                    $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} - {activeTrade.BuyPrice} = {currentTrade.Close - activeTrade.BuyPrice} ");

                await Sell(data, activeTrade);
                data.StrategyInstance.Status = $"Sold! {activeTrade.SellPrice} at profit {activeTrade.Profit}";
            }
            else
            {
                data.StrategyInstance.Status = $"Waiting for price above {MoveProfit(data)} or stop loss {StopLoss(data)}";
            }
        }

        private async Task<decimal?> MoveProfit(StrategyContext data, decimal? setValue = null)
        {
            var key = "MoveProfit";
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _moveProfitPercent;
                return await data.Get(key, moveProfitPercent);
            }
            await data.Set(key, setValue.Value);
            return setValue;
        }

        protected async Task<decimal?> StopLoss(StrategyContext data, decimal? setValue = null)
        {
            var key = "StopLoss";
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _initialStopRisk;
                return await data.Get(key, moveProfitPercent);
            }
            await data.Set(key, setValue.Value);
            return setValue;
        }

        protected async Task<decimal> ResetStops(StrategyContext data, decimal currentTradeClose)
        {
            var initialStopRisk = currentTradeClose * _initialStopRisk;
            await StopLoss(data, initialStopRisk);
            await MoveProfit(data, currentTradeClose * _moveProfitPercent);
            return initialStopRisk;
        }
    }
}