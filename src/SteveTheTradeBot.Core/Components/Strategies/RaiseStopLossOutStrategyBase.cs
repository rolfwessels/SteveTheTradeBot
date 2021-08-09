using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
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

        protected async Task RaiseStopLoss(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade)
        {
            if (currentTrade.Close > MoveProfit(data))
            {
                ResetStops(currentTrade, data);
                data.StrategyInstance.Status = $"Update stop loss to {StopLoss(data)}";
                await data.Messenger.Send(
                    $"{data.StrategyInstance.Name} has updated its stop loss to {StopLoss(data)}");
            }
            else if (currentTrade.Close <= StopLoss(data))
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

        private decimal? MoveProfit(StrategyContext data, decimal? setValue = null)
        {
            var key = "MoveProfit";
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _moveProfitPercent;
                return data.Get(key, moveProfitPercent).Result;
            }
            data.Set(key, setValue.Value).Wait();
            return setValue;
        }

        protected decimal? StopLoss(StrategyContext data, decimal? setValue = null)
        {
            var key = "StopLoss";
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _initialStopRisk;
                return data.Get(key, moveProfitPercent).Result;
            }
            data.Set(key, setValue.Value).Wait();
            return setValue;
        }

        protected void ResetStops(TradeQuote currentTrade, StrategyContext data)
        {
            StopLoss(data, currentTrade.Close * _initialStopRisk);
            MoveProfit(data, currentTrade.Close * _moveProfitPercent);
        }
    }
}