using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Components.Bots
{
    public abstract class BaseBot : IBot
    {
        private readonly IBrokerApi _broker;

        protected BaseBot(IBrokerApi broker)
        {
            _broker = broker;
        }

        #region Implementation of IBot

        public abstract Task DataReceived(BackTestRunner.BotData data);
        public abstract string Name { get; }

        #endregion

        public async Task<Trade> Buy(BackTestRunner.BotData trade, decimal randValue)
        {
            var currentTrade = trade.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var quantity = randValue / estimatedPrice;
            var addTrade = trade.BackTestResult.AddTrade(currentTrade.Date, estimatedPrice, quantity, randValue);
            var tradeOrder = addTrade.AddOrderRequest(Side.Buy, randValue, estimatedPrice , quantity, trade.BackTestResult.CurrencyPair, currentTrade.Date);
            var response = await _broker.MarketOrder(tradeOrder.ToMarketOrderRequest());
            tradeOrder.Apply(response);
            await trade.PlotRunData(currentTrade.Date, "activeTrades", trade.BackTestResult.TradesActive);
            return addTrade;
        }

        public async Task Sell(BackTestRunner.BotData trade, Trade activeTrade, IQuote currentTrade)
        {
            var close = activeTrade.Close(currentTrade.Date, currentTrade.Close);
            trade.BackTestResult.ClosingBalance = close.Value;
            await _broker.MarketOrder(new MarketOrderRequest(Side.Sell, 0.1m, trade.BackTestResult.CurrencyPair, activeTrade.Id+"_out", currentTrade.Date));
            await trade.PlotRunData(currentTrade.Date, "activeTrades", trade.BackTestResult.TradesActive);
            await trade.PlotRunData(currentTrade.Date, "sellPrice", close.Value);
        }
    }
}