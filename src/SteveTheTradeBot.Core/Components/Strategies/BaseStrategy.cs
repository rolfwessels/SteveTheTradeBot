using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Hangfire.Logging;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public abstract class BaseStrategy : IStrategy
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        #region Implementation of IBot

        public abstract Task DataReceived(StrategyContext data);
        public abstract string Name { get; }
        public Task SellAll(StrategyContext strategyContext)
        {
            var enumerable = strategyContext.StrategyInstance.Trades.Where(x => x.IsActive).Select(x => Sell(strategyContext, x));
            return Task.WhenAll(enumerable);
        }

        #endregion

        public async Task<StrategyTrade> Buy(StrategyContext data, decimal randValue)
        {
            var currentTrade = data.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = randValue / estimatedPrice;
            var addTrade = data.StrategyInstance.AddTrade(currentTrade.Date, estimatedPrice, estimatedQuantity, randValue);
            var tradeOrder = addTrade.AddOrderRequest(Side.Buy, randValue, estimatedPrice , estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date);
            try
            {
                var response = await data.Broker.Order(BrokerUtils.ToOrderRequest(tradeOrder));
                BrokerUtils.ApplyValue(tradeOrder, response, Side.Sell);
                addTrade.ApplyBuyInfo(tradeOrder);
                await data.PlotRunData(currentTrade.Date, "activeTrades", 1);
                await data.PlotRunData(currentTrade.Date, "sellPrice", randValue);
                await data.Messenger.Send(new TradeOrderMadeMessage(data.StrategyInstance, addTrade, tradeOrder));
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                BrokerUtils.Close(addTrade, currentTrade.Date, tradeOrder);
                _log.Error(e, $"Failed to add new trade order:{e.Message}");
            }
            return addTrade;
        }

        public async Task Sell(StrategyContext data, StrategyTrade activeTrade)
        {
            var currentTrade = data.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = activeTrade.BuyQuantity * estimatedPrice;
            var tradeOrder = activeTrade.AddOrderRequest(Side.Sell, activeTrade.BuyQuantity, estimatedPrice, estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date);
            try
            {
                var response = await data.Broker.Order(BrokerUtils.ToOrderRequest(tradeOrder));
                BrokerUtils.ApplyValue(tradeOrder, response, Side.Buy);
                var close = BrokerUtils.Close(activeTrade, currentTrade.Date, tradeOrder);
                await data.PlotRunData(currentTrade.Date, "activeTrades", 0);

                data.StrategyInstance.BaseAmount = close.SellValue;
                await data.PlotRunData(currentTrade.Date, "sellPrice", close.SellValue);
                await data.Messenger.Send(new TradeOrderMadeMessage(data.StrategyInstance, activeTrade, tradeOrder));
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                _log.Error(e, $"Failed to add close trade order:{e.Message}");
            }
            
        }
    }
}
