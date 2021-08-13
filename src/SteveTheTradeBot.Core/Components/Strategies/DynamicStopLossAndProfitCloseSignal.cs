using System;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class DynamicStopLossAndProfitCloseSignal : ICloseSignal 
    {
        public DynamicStopLossAndProfitCloseSignal()
        {
            MaxRisk = 0.02m;
            MinRisk = 0.01m;
            TimesTheRisk = 3;
        }

        public decimal TimesTheRisk { get;  }

        public decimal MaxRisk { get; }
        public decimal MinRisk { get; }

        #region Implementation of ICloseSignal

        public async Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice, BaseStrategy strategy)
        {
            var highest = data.ByMinute.Select((x,i)=> new {i,x}).OrderByDescending(x=>x.x.High).First();
            var lowest = data.ByMinute.Skip(highest.i).OrderBy(x => x.Low).First();
            var stopLost = Math.Min(LeastAmountOfRisk(boughtAtPrice), Math.Max(lowest.Low, MaxAmountOfRisk(boughtAtPrice)));
            await strategy.SetStopLoss(data, stopLost);

            var risk = TradeUtils.MovementPercent(boughtAtPrice, stopLost)/100;
            await data.Set("BoughtAtPrice", boughtAtPrice);
            await data.Set("StopLoss",stopLost);
            await data.Set("Risk", risk);
            await data.Set("NextStopLoss", (1+ risk) * boughtAtPrice);
            await data.Set("ExitAt", (1 + risk*TimesTheRisk) * boughtAtPrice);
            return stopLost;
        }

        private decimal LeastAmountOfRisk(decimal boughtAtPrice)
        {
            return (1m - MinRisk) * boughtAtPrice;
        }

        private decimal MaxAmountOfRisk(decimal boughtAtPrice)
        {
            return (1m - MaxRisk) * boughtAtPrice;
        }

        public async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade,
            BaseStrategy strategy)
        {
            if (currentTrade.Close > await data.Get("ExitAt", 0))
            {
                await strategy.Sell(data, activeTrade);
            }
            else if (currentTrade.Close > await data.Get("NextStopLoss", 0))
            {
                var boughtAtPrice = await data.Get("BoughtAtPrice", 0);
                await strategy.SetStopLoss(data, boughtAtPrice);
            }

        }

        #endregion
    }
}