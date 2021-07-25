using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Notifications;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Notifications
{
    public class MessageToNotificationTests
    {
        private MessageToNotification _messageToNotification;
        private FakeNotification _fakeNotification;

        [Test]
        public async Task OnTradeOrderMade_GivenAPurchase_ShouldPostToSlackChannel()
        {
            // arrange
            Setup();
            var forBackTest = StrategyInstance.ForBackTest("RsiIsAThing",CurrencyPair.XRPZAR);
            var strategyTrade = SetupTrade(forBackTest, out var addOrderRequest);

            // action
            await _messageToNotification.OnTradeOrderMade(new TradeOrderMadeMessage(forBackTest, strategyTrade,
                addOrderRequest));
            // assert
            _fakeNotification.Post.Should().HaveCount(1);
            _fakeNotification.Post.First().Should().Be("RsiIsAThing just *bought* 0.00162178XRP for *R200* at R123321! :robot_face:");
        }
        
        [Test]
        public async Task OnTradeOrderMade_GivenAProfitableSale_ShouldShowMessage()
        {
            // arrange
            Setup();
            var forBackTest = StrategyInstance.ForBackTest("RsiIsAThing",CurrencyPair.XRPZAR);
            var strategyTrade = SetupTrade(forBackTest, out var addOrderRequest);
            
            var estimatedPrice = addOrderRequest.PriceAtRequest * 1.1m;
            var tradeOrder = CloseTrade(strategyTrade, estimatedPrice, forBackTest);
            // action
            await _messageToNotification.OnTradeOrderMade(new TradeOrderMadeMessage(forBackTest, strategyTrade, tradeOrder));
            // assert
            _fakeNotification.PostSuccess.Should().HaveCount(1);
            _fakeNotification.PostSuccess.First().Should().Be("RsiIsAThing just *sold* *0.00162178XRP* for R220.00 at R135653.1! We made R20.00 :moneybag:");
        }
 
        [Test]
        public async Task OnTradeOrderMade_GivenALossSale_ShouldShowMessage()
        {
            // arrange
            Setup();
            var forBackTest = StrategyInstance.ForBackTest("RsiIsAThing",CurrencyPair.XRPZAR);
            var strategyTrade = SetupTrade(forBackTest, out var addOrderRequest);
            
            var estimatedPrice = addOrderRequest.PriceAtRequest * 0.9m;
            var tradeOrder = CloseTrade(strategyTrade, estimatedPrice, forBackTest);
            // action
            await _messageToNotification.OnTradeOrderMade(new TradeOrderMadeMessage(forBackTest, strategyTrade, tradeOrder));
            // assert
            _fakeNotification.PostFailed.Should().HaveCount(1);
            _fakeNotification.PostFailed.First().Should().Be("RsiIsAThing just *sold* *0.00162178XRP* for R180.00 at R110988.9! We lost R20.00 :money_with_wings:");
        }

        private static TradeOrder CloseTrade(StrategyTrade strategyTrade, decimal estimatedPrice, StrategyInstance forBackTest)
        {
            var estimatedQuantity = strategyTrade.BuyQuantity * estimatedPrice;
            var tradeOrder = strategyTrade.AddOrderRequest(Side.Sell, strategyTrade.BuyQuantity, estimatedPrice,
                estimatedQuantity, forBackTest.Pair, DateTime.Now);
            BrokerUtils.Close(strategyTrade, tradeOrder.CreateDate, tradeOrder);
            return tradeOrder;
        }

        private static StrategyTrade SetupTrade(StrategyInstance forBackTest, out TradeOrder addOrderRequest)
        {
            var price = 123321m;
            var amount = 200m;
            var estimatedQuantity = Math.Round(amount / price, 8);

            var strategyTrade = forBackTest.AddTrade(DateTime.Now, price, estimatedQuantity, amount);
            addOrderRequest =
                strategyTrade.AddOrderRequest(Side.Buy, amount, price, estimatedQuantity, forBackTest.Pair, DateTime.Now);
            return strategyTrade;
        }

        private void Setup()
        {
            _fakeNotification = new FakeNotification();
            _messageToNotification = new MessageToNotification(_fakeNotification);
        }


        internal class FakeNotification : INotificationChannel
        {
            public readonly List<string> Post = new List<string>();
            public readonly List<string> PostSuccess = new List<string>();
            public readonly List<string> PostFailed = new List<string>();

            #region Implementation of INotificationChannel

            public Task PostAsync(string message)
            {
                Post.Add(message);
                return Task.CompletedTask;
            }

            public Task PostSuccessAsync(string message)
            {
                PostSuccess.Add(message);
                return Task.CompletedTask;
            }

            public Task PostFailedAsync(string message)
            {
                PostFailed.Add(message);
                return Task.CompletedTask;
            }

            #endregion
        }
    }

}