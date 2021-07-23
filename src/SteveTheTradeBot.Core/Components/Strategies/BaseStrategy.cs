using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public abstract class BaseStrategy : IStrategy
    {
        private readonly IBrokerApi _broker;

        protected BaseStrategy(IBrokerApi broker)
        {
            _broker = broker;
        }

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
            var response = await _broker.Order(BrokerUtils.ToOrderRequest(tradeOrder));
            BrokerUtils.ApplyValue(tradeOrder, response, Side.Sell);
            addTrade.ApplyBuyInfo(tradeOrder);
            await data.PlotRunData(currentTrade.Date.AddMinutes(-1), "activeTrades", 0);
            await data.PlotRunData(currentTrade.Date, "activeTrades", 1);
            await data.PlotRunData(currentTrade.Date, "sellPrice", randValue);
            return addTrade;
        }

        public async Task Sell(StrategyContext data, StrategyTrade activeTrade)
        {
            var currentTrade = data.LatestQuote();
            var currentTradeDate = currentTrade.Date;
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = activeTrade.BuyQuantity * estimatedPrice;
            var tradeOrder = activeTrade.AddOrderRequest(Side.Sell, activeTrade.BuyQuantity, estimatedPrice, estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date);
            var response = await _broker.Order(BrokerUtils.ToOrderRequest(tradeOrder));
            
            BrokerUtils.ApplyValue(tradeOrder, response, Side.Buy);
            var close = BrokerUtils.Close(activeTrade, currentTradeDate, tradeOrder);

            await data.PlotRunData(currentTradeDate.AddMinutes(-1), "activeTrades", 1);
            await data.PlotRunData(currentTradeDate, "activeTrades", 0);
            
            data.StrategyInstance.BaseAmount = close.SellValue;
            await data.PlotRunData(currentTradeDate, "sellPrice", close.SellValue);
        }
    }
}