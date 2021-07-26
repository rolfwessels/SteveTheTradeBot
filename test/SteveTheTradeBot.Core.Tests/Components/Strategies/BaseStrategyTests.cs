using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class BaseStrategyTests
    {
        private FakeStrategy _fakeStrategy;
        private StrategyContext _data;
        private FakeBroker _fakeBroker;
        private DateTime _requestDate;

        #region Setup/Teardown

        public void Setup()
        {
            _requestDate = new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc);
            _fakeBroker = new FakeBroker().With(x => x.BuyFeePercent = 0.0075m);
            _data = new StrategyContext(new DynamicGraphsTests.FakeGraph(), StrategyInstance.ForBackTest("BTCZAR", CurrencyPair.BTCZAR), _fakeBroker, Messenger.Default);
            var tradeFeedCandles = Builder<TradeFeedCandle>.CreateListOfSize(4)
                .WithValidData()
                .All().With((x, r) => x.Date = _requestDate.AddDays(-1 * r))
                .Build();

            _data.ByMinute.AddRange(tradeFeedCandles);
            _fakeStrategy = new FakeStrategy();
        }

        #endregion

        [Test]
        public async Task Buy_Given100Rand_ShouldAddTradeWithDetail()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;
            var expectedTrade = SetupTrade(close, buyValue);
            expectedTrade.BuyPrice = _fakeBroker.AskPrice;
            expectedTrade.FeeAmount = expectedTrade.BuyValue * _fakeBroker.BuyFeePercent;
            expectedTrade.FeeCurrency = "ZAR";
            // action
            var trade = await _fakeStrategy.Buy(_data, buyValue);
            // assert
            trade.Should().BeEquivalentTo(expectedTrade.Dump("expectedTrade"),
                e => e.Excluding(x => x.Orders)
                    .Excluding(x => x.Id)
                    .Excluding(x => x.FeeAmount)
                    .Excluding(x => x.BuyQuantity)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));

            trade.BuyQuantity.Should().BeApproximately(expectedTrade.BuyQuantity, 0.01m);
            trade.FeeAmount.Should().BeApproximately(expectedTrade.FeeAmount, 0.01m);
        }


        [Test]
        public async Task Sell_Given100Rand_ShouldAddTradeWithDetail()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;
            var expectedTrade = SetupTrade(close, buyValue);
            expectedTrade.BuyPrice = _fakeBroker.AskPrice;
            expectedTrade.FeeAmount = expectedTrade.BuyValue * _fakeBroker.BuyFeePercent;
            expectedTrade.FeeCurrency = "ZAR";
            var trade = await _fakeStrategy.Buy(_data, buyValue);
            AddCandle(close - 5, new DateTime(2001, 01, 01, 3, 2, 4, DateTimeKind.Utc));

            // action
            await _fakeStrategy.Sell(_data, trade);
            // assert
            trade.EndDate.Should().Be(new DateTime(2001, 01, 01, 3, 2, 4, DateTimeKind.Utc));
            trade.SellValue.Should().Be(98.59m);
            trade.SellPrice.Should().BeApproximately(100100, 1m);
            trade.Profit.Should().Be(-1.410m);
            trade.IsActive.Should().Be(false);
            trade.FeeAmount.Should().BeApproximately(1.49m, 0.1m);
        }

        [Test]
        public async Task Buy_Given100Rand_ShouldMakeMarketRequest()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;
            var expectedTrade = SetupTrade(close, buyValue);
            // action
            var trade = await _fakeStrategy.Buy(_data, buyValue);
            // assert
            var marketOrderRequest = _fakeBroker.Requests
                .OfType<SimpleOrderRequest>().First();


            marketOrderRequest.PayInCurrency.Should().Be("ZAR");
            marketOrderRequest.PayAmount.Should().Be(100);
            marketOrderRequest.Side.Should().Be(Side.Buy);
            marketOrderRequest.CustomerOrderId.Should().Be(trade.Orders.First().Id);
            marketOrderRequest.RequestDate.Should().BeSameDateAs(DateTime.Parse("2001-01-01T01:02:03Z"));
            marketOrderRequest.CurrencyPair.Should().Be(CurrencyPair.BTCZAR);
        }

        [Test]
        public async Task Sell_Given100Rand_ShouldMakeMarketRequest()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();

            // action
            await _fakeStrategy.Sell(_data, addTrade);
            // assert
            var marketOrderRequest = _fakeBroker.Requests
                .OfType<SimpleOrderRequest>().First();

            marketOrderRequest.PayInCurrency.Should().Be("BTC");
            marketOrderRequest.PayAmount.Should().Be(quantityBought);
            marketOrderRequest.Side.Should().Be(Side.Sell);
            marketOrderRequest.CustomerOrderId.Should().HaveLength(32);
            marketOrderRequest.RequestDate.Should().BeSameDateAs(dateTime);
            marketOrderRequest.CurrencyPair.Should().Be(CurrencyPair.BTCZAR);
        }


        [Test]
        public async Task SetStopLoss_Given100Rand_ShouldMakeMarketRequest()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            // action
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            // assert
            var request = _fakeBroker.Requests
                .OfType<StopLimitOrderRequest>().First();

            request.Price.Should().Be(90000);
            request.StopPrice.Should().Be(89100);
            request.Type.Should().Be(StopLimitOrderRequest.Types.StopLossLimit);
            request.Quantity.Should().Be(quantityBought);
            request.Side.Should().Be(Side.Sell);
            request.CustomerOrderId.Should().HaveLength(32);
            request.TimeInForce.Should().Be(TimeEnforce.FillOrKill);
            request.Pair.Should().Be(CurrencyPair.BTCZAR);
        }


        [Test]
        public async Task SetStopLoss_Given100Rand_ShouldIfStopLossWasASuccessUpdateTheTradeRecord()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            var validStopLoss = _data.ActiveTrade().GetValidStopLoss();
            var activeTrade = _data.ActiveTrade();
            // action
            _fakeBroker.ActivateStopLoss(_data, _data.ActiveTrade(), validStopLoss);
            // assert
            
            activeTrade.IsActive.Should().BeFalse();
            validStopLoss.RequestDate.Should().Be(_requestDate);
            validStopLoss.OrderStatusType.Should().Be(OrderStatusTypes.Filled);
            validStopLoss.CurrencyPair.Should().Be("BTCZAR");
            validStopLoss.OrderPrice.Should().Be(90000.0M);
            validStopLoss.RemainingQuantity.Should().Be(0);
            validStopLoss.OriginalQuantity.Should().Be(88.43M);
            validStopLoss.OrderSide.Should().Be(Side.Sell);
            validStopLoss.OrderType.Should().Be("stop-loss");
            validStopLoss.FailedReason.Should().Be(null);
            validStopLoss.PriceAtRequest.Should().Be(89100);
            validStopLoss.OutQuantity.Should().Be(quantityBought);
            validStopLoss.OutCurrency.Should().Be("BTC");
            validStopLoss.FeeCurrency.Should().Be("ZAR");
            validStopLoss.FeeAmount.Should().Be(0.67M);
        }


        [Test]
        public async Task SetStopLoss_Given100Rand_ShouldIfStopLossWasASuccessUpdateActiveTrade()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            var validStopLoss = _data.ActiveTrade().GetValidStopLoss();
            var trade = _data.ActiveTrade();
            // action
            _fakeBroker.ActivateStopLoss(_data, _data.ActiveTrade(), validStopLoss);
            // assert
            trade.IsActive.Should().BeFalse();
            trade.EndDate.Should().Be(new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc));
            trade.SellValue.Should().Be(88.43m);
            trade.SellPrice.Should().BeApproximately(90000, 1m);
            trade.Profit.Should().Be(-11.57m);
            trade.IsActive.Should().Be(false);
            trade.FeeCurrency.Should().Be("ZAR");
            trade.FeeAmount.Should().BeApproximately(1.57m, 0.01m);
        }

        [Test]
        public async Task SetStopLoss_GivenFailedApiCall_ShouldFailTheOrder()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            _fakeBroker.Throw(new Exception("nope"));
            // action
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            // assert
            var last = _data.ActiveTrade().Orders.Last();
            last.OrderStatusType.Should().Be(OrderStatusTypes.Failed);
            last.FailedReason.Should().Be("nope");
        }


        [Test]
        public async Task SetStopLoss_GivenAnExistingStopLoss_ShouldCancelThatStopLoss()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount * 0.9m);
            var prev = _data.ActiveTrade().GetValidStopLoss();
            // action
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            // assert
            var last = _data.ActiveTrade().GetValidStopLoss();

            prev.OrderStatusType.Should().Be(OrderStatusTypes.Cancelled);
            last.OrderPrice.Should().Be(90000.0m);

        }

        [Test]
        public async Task SetStopLoss_GivenWhenSet_ShouldAddOrder()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            // action
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            // assert
            var last = _data.ActiveTrade().GetValidStopLoss();

            last.CurrencyPair.Should().Be("BTCZAR");
            last.OrderPrice.Should().Be(90000.0m);
            last.RemainingQuantity.Should().Be(0);
            last.OriginalQuantity.Should().Be(0);
            last.OrderSide.Should().Be(0);
            last.OrderType.Should().Be("stop-loss");
            last.BrokerOrderId.Should().EndWith("-req");
            last.FailedReason.Should().Be(null);
            last.PriceAtRequest.Should().Be(89100M);
            last.OutQuantity.Should().Be(0.001m);
            last.OutCurrency.Should().Be("BTC");
            last.FeeAmount.Should().Be(0);
            last.FeeCurrency.Should().Be("ZAR");
            last.OrderStatusType.Should().Be(OrderStatusTypes.Placed);
        }

        private (DateTime dateTime, decimal quantityBought, StrategyTrade addTrade) SetupSale()
        {
            var close = 100000;
            var buyValue = 100m;
            SetupTrade(close, buyValue);
            var dateTime = DateTime.Parse("2001-01-01T01:02:03Z");
            var (addTrade, tradeOrder) = _data.StrategyInstance.AddBuyTradeOrder(buyValue, close, dateTime);
            addTrade.FeeAmount = 0.9m;
            return (dateTime, tradeOrder.OriginalQuantity, addTrade);
        }

        [Test]
        public async Task Buy_Given100Rand_ShouldAddOrder()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;

            SetupTrade(close, buyValue);
            // action
            var trade = await _fakeStrategy.Buy(_data, buyValue);

            var expectedOrder = new TradeOrder()
            {
                RequestDate = _requestDate,
                OrderStatusType = OrderStatusTypes.Filled,
                CurrencyPair = "BTCZAR",
                OrderPrice = _fakeBroker.AskPrice,
                RemainingQuantity = 0,
                OriginalQuantity = 0.00099240M,
                OrderSide = Side.Buy,
                OrderType = "simple",
                BrokerOrderId = trade.Orders.First().Id + "_broker",
                FailedReason = null,
                PriceAtRequest = 100000,
                OutQuantity = 100,
                OutCurrency = "ZAR",
                FeeCurrency = "BTC",
                FeeAmount = 0.000007499250M,
            };
            // assert

            trade.Orders.First().Should().BeEquivalentTo(expectedOrder,
                e => e
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));
        }


        [Test]
        public async Task Sell_Given100Rand_ShouldAddOrder()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var expectedOrder = new TradeOrder()
            {
                RequestDate = _requestDate,
                OrderStatusType = OrderStatusTypes.Filled,
                CurrencyPair = "BTCZAR",
                OrderPrice = _fakeBroker.BidPrice,
                RemainingQuantity = 0,
                OriginalQuantity = 99.35M,
                OrderSide = Side.Sell,
                OrderType = "simple",
                FailedReason = null,
                PriceAtRequest = 100000,
                OutQuantity = quantityBought,
                OutCurrency = "BTC",
                FeeCurrency = "ZAR",
                FeeAmount = 0.75M,
            };
            // action
            await _fakeStrategy.Sell(_data, addTrade);
            // assert

            addTrade.Orders.Last().Dump("last").Should().BeEquivalentTo(expectedOrder,
                e => e
                    .Excluding(x => x.Id)
                    .Excluding(x => x.BrokerOrderId)
                    .Excluding(x => x.FeeAmount)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));
            addTrade.Orders.Last().FeeAmount.Should().BeApproximately(expectedOrder.FeeAmount, 0.1m);
        }

        private StrategyTrade SetupTrade(int close, decimal buyValue)
        {
            var tradeFeedCandle = AddCandle(close, _requestDate);
            var expectedTrade = new StrategyTrade(tradeFeedCandle.Date, close, buyValue / close, buyValue);

            return expectedTrade;
        }

        private TradeFeedCandle AddCandle(int close, DateTime dateTime)
        {
            var tradeFeedCandle = Builder<TradeFeedCandle>.CreateNew().WithValidData()
                .With(x => x.Close = close)
                .With(x => x.Date = dateTime)
                .Build();
            _data.ByMinute.Add(tradeFeedCandle);
            return tradeFeedCandle;
        }
    }


    public class FakeStrategy : BaseStrategy
    {
        public List<StrategyContext> DateRecievedValues = new List<StrategyContext>();

        #region Overrides of BaseBot

        public override Task DataReceived(StrategyContext data)
        {
            DateRecievedValues.Add(data);
            return Task.CompletedTask;
        }

        public override string Name { get; } = "Test";

        #endregion
    }
}