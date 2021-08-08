using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiStrategy : BaseStrategy
    {
        public const string Desc = "SimpleRsi";

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly int _buySignal;
        private readonly decimal _initialStopRisk;
        private readonly decimal _buy200rocsma;
        private readonly decimal _moveProfitPercent;
        private readonly decimal _secondStopRisk;

        public RSiStrategy()
        {
            _initialStopRisk = 0.96m;
            _secondStopRisk = 0.96m;
            _moveProfitPercent = 1.03m;
            _buySignal = 30;
            _buy200rocsma = 0.5m;
        }
        
        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();
            var rsiResults = currentTrade.Metric.GetOrDefault("rsi14");
            var roc200sma = currentTrade.Metric.GetOrDefault("roc200-sma");
            var roc100sma = currentTrade.Metric.GetOrDefault("roc100-sma");
            if (activeTrade == null)
            {
                if (rsiResults < _buySignal && (roc200sma.HasValue && roc200sma.Value > _buy200rocsma && roc100sma.Value > _buy200rocsma))
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults} Rsi:{roc200sma.Value}");
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    var lossAmount = strategyTrade.BuyPrice * _initialStopRisk;
                    await data.Messenger.Send(new PostSlackMessage()
                        { Message = $"{data.StrategyInstance.Name} set stop loss to {lossAmount}." });
                    await SetStopLoss(data, lossAmount);
                    data.StrategyInstance.Status =
                        $"Bought! [{strategyTrade.BuyPrice} and set stop loss at {lossAmount}]";
                }
                else
                {
                    data.StrategyInstance.Status =
                        $"Waiting to buy ![{rsiResults} < {_buySignal}] [{roc200sma} > {_buy200rocsma}]";
                }
            }
            else
            {
                var moveProfit = GetMoveProfit(activeTrade);
                if (currentTrade.Close > moveProfit)
                {
                    var lossAmount = currentTrade.Close * _secondStopRisk;
                    await data.Messenger.Send(new PostSlackMessage()
                        { Message = $"{data.StrategyInstance.Name} update stop loss to {lossAmount}. :chart_with_upwards_trend: " });
                    await SetStopLoss(data, lossAmount);
                    data.StrategyInstance.Status = $"Update stop loss to {lossAmount}";
                }
                else
                {
                    data.StrategyInstance.Status = $"Waiting for price above {moveProfit} or stop loss {activeTrade.GetValidStopLoss()?.OrderPrice}]";
                }
            }
        }


        private decimal GetMoveProfit(StrategyTrade activeTrade)
        {
            var validStopLoss = activeTrade.GetValidStopLoss();
            if (validStopLoss != null)
            {
                var moveProfitPercent = validStopLoss.OrderPrice * (_moveProfitPercent + (1 - _initialStopRisk));
                return moveProfitPercent;
            }
            return activeTrade.BuyPrice;
        }


        public override string Name => Desc;
      
    }
}
