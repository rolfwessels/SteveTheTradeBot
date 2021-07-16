﻿using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

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

        public async Task<Trade> Buy(BackTestRunner.BotData data, decimal randValue)
        {
            var currentTrade = data.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = randValue / estimatedPrice;
            var addTrade = data.BackTestResult.AddTrade(currentTrade.Date, estimatedPrice, estimatedQuantity, randValue);
            var tradeOrder = addTrade.AddOrderRequest(Side.Buy, randValue, estimatedPrice , estimatedQuantity, data.BackTestResult.CurrencyPair, currentTrade.Date);
            var response = await _broker.Order(tradeOrder.ToOrderRequest());
            tradeOrder.ApplyValue(response, Side.Sell);
            addTrade.ApplyBuyInfo(tradeOrder);
            await data.PlotRunData(currentTrade.Date, "activeTrades", data.BackTestResult.TradesActive);
            return addTrade;
        }

        public async Task Sell(BackTestRunner.BotData data, Trade activeTrade)
        {
            var currentTrade = data.LatestQuote();
            var currentTradeDate = currentTrade.Date;
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = activeTrade.Quantity * estimatedPrice;
            var tradeOrder = activeTrade.AddOrderRequest(Side.Sell, activeTrade.Quantity, estimatedPrice, estimatedQuantity, data.BackTestResult.CurrencyPair, currentTrade.Date);
            var response = await _broker.Order(tradeOrder.ToOrderRequest());
            
            tradeOrder.ApplyValue(response, Side.Buy);
            var close = activeTrade.Close(currentTradeDate, tradeOrder);

            await data.PlotRunData(currentTradeDate, "activeTrades", data.BackTestResult.TradesActive);
            
            data.BackTestResult.ClosingBalance = close.Value;

            await data.PlotRunData(currentTradeDate, "sellPrice", close.Value);
        }
    }
}