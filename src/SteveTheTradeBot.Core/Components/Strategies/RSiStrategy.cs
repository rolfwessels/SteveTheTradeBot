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
            _initialStopRisk = 0.94m;
            _secondStopRisk = 0.94m;
            _moveProfitPercent = 1.15m;
            _buySignal = 30;
            _buy200rocsma = 0.5m;
        }
        
        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();
            var rsiResults = currentTrade.Metric.GetOrDefault("rsi14");
            var roc200sma = currentTrade.Metric.GetOrDefault("roc200-sma");
            if (activeTrade == null)
            {
                if (rsiResults < _buySignal && (roc200sma.HasValue && roc200sma.Value > _buy200rocsma))
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults} Rsi:{roc200sma.Value}");
                    var strategyTrade = await Buy(data, data.StrategyInstance.BaseAmount);
                    var lossAmount = strategyTrade.BuyPrice * _initialStopRisk;
                    await data.Messenger.Send(new PostSlackMessage()
                        { Message = $"{data.StrategyInstance.Reference} set stop loss to {lossAmount}." });
                    await SetStopLoss(data, lossAmount);
                }
            }
            else
            {
                if (currentTrade.Close > GetMoveProfit(activeTrade))
                {
                    var lossAmount = currentTrade.Close * _secondStopRisk;
                    await data.Messenger.Send(new PostSlackMessage()
                        { Message = $"{data.StrategyInstance.Reference} update stop loss to {lossAmount}." });
                    await SetStopLoss(data, lossAmount);
                }

                var validStopLoss = activeTrade.GetValidStopLoss();
                if (validStopLoss != null && currentTrade.Low <= validStopLoss.OrderPrice)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} - {activeTrade.BuyPrice} = {currentTrade.Close - activeTrade.BuyPrice} Rsi:{rsiResults}");
                
                    await Sell(data, activeTrade);
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
