using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using Flurl.Http;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Tests.Components.Storage;
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
            _fakeBroker = new FakeBroker(Messenger.Default).With(x => x.BuyFeePercent = 0.0075m);
            _data = new StrategyContext(new DynamicGraphsTests.FakeGraph(), StrategyInstance.ForBackTest("BTCZAR", CurrencyPair.BTCZAR), _fakeBroker, Messenger.Default,new ParameterStore(TestTradePersistenceFactory.InMemoryDb));
            var tradeFeedQuotes = Builder<TradeQuote>.CreateListOfSize(4)
                .WithValidData()
                .All().With((x, r) => x.Date = _requestDate.AddDays(-1 * r))
                .Build();

            _data.ByMinute.AddRange(tradeFeedQuotes);
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
            expectedTrade.FeeAmount = 0.0000074m;
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

            trade.BuyQuantity.Should().BeApproximately(0.001m, 0.01m);
            trade.FeeAmount.Should().BeApproximately(0.74m, 0.01m);
        }

        [Test]
        public async Task Buy_Given100Rand_ShouldSetTheBuyQuantityToTheAmounMinusTheFee()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;
            var expectedTrade = SetupTrade(close, buyValue);
            expectedTrade.BuyPrice = _fakeBroker.AskPrice;
            expectedTrade.FeeAmount = 0.0000074m;
            expectedTrade.FeeCurrency = "ZAR";
            // action
            var trade = await _fakeStrategy.Buy(_data, buyValue);
            // assert
            trade.BuyQuantity.Should().Be(trade.Orders[0].OriginalQuantity - trade.Orders[0].TotalFee);
        }

        [Test]
        public async Task Sell_Given100Rand_ShouldAddTradeWithDetail()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;
            SetupTrade(close, buyValue);
            var trade = await _fakeStrategy.Buy(_data, buyValue);
            AddCandle(close - 5, new DateTime(2001, 01, 01, 3, 2, 4, DateTimeKind.Utc));
            trade.Orders.Last().FeeCurrency.Should().Be("BTC");
            trade.FeeAmount.Should().BeApproximately(0.74m, 0.1m);
            // action
            await _fakeStrategy.Sell(_data, trade);
            // assert
            _data.StrategyInstance.Print();
            trade.Dump("sell");
            trade.EndDate.Should().BeCloseTo(new DateTime(2001, 01, 01, 3, 2, 4, DateTimeKind.Utc),5000);
            trade.SellValue.Should().Be(98.59m);
            trade.SellPrice.Should().BeApproximately(100100, 2m);
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
            request.StopPrice.Should().Be(90090);
            request.Type.Should().Be(StopLimitOrderRequest.Types.StopLossLimit);
            request.Quantity.Should().Be(quantityBought);
            request.Side.Should().Be(Side.Sell);
            request.CustomerOrderId.Should().HaveLength(32);
            request.TimeInForce.Should().Be(TimeEnforce.FillOrKill);
            request.Pair.Should().Be(CurrencyPair.BTCZAR);
        }


        [Test]
        public async Task ActivateStopLoss_Given100Rand_ShouldIfStopLossWasASuccessUpdateTheTradeRecord()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            var validStopLoss = _data.ActiveTrade().GetValidStopLoss();
            var activeTrade = _data.ActiveTrade();
            // action
            await BrokerUtils.ApplyOrderResultToStrategy(_data, _data.ActiveTrade(), validStopLoss, _fakeBroker.BuyFeePercent);
            // assert
            
            activeTrade.IsActive.Should().BeFalse();
            validStopLoss.OrderType.Should().Be(StrategyTrade.OrderTypeStopLoss);
            validStopLoss.RequestDate.Should().Be(_requestDate);
            validStopLoss.OrderStatusType.Should().Be(OrderStatusTypes.Filled);
            validStopLoss.CurrencyPair.Should().Be("BTCZAR");
            validStopLoss.OrderPrice.Should().Be(90000.0M);
            validStopLoss.RemainingQuantity.Should().Be(0);
            validStopLoss.OriginalQuantity.Should().Be(quantityBought);
            validStopLoss.OrderSide.Should().Be(Side.Sell);
            validStopLoss.FailedReason.Should().Be(null);
            validStopLoss.PriceAtRequest.Should().Be(100000M);
            validStopLoss.Total.Should().Be(89.32M);
            validStopLoss.FeeCurrency.Should().Be("ZAR");
            validStopLoss.TotalFee.Should().Be(0.68M);
        }

        [Test]
        public async Task ActivateStopLoss_GivenFailedStopLoss_ShouldCancelTheStopLoss()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            var validStopLoss = _data.ActiveTrade().GetValidStopLoss();
            var activeTrade = _data.ActiveTrade();
            var orderStatusById = new OrderHistorySummaryResponse { OrderStatusType = "Failed", FailedReason = "Failed?"};
            // action
            await BrokerUtils.ApplyOrderResultToStrategy(_data, _data.ActiveTrade(), validStopLoss, orderStatusById);
            // assert
            validStopLoss.OrderStatusType.Should().Be(OrderStatusTypes.Failed);
            validStopLoss.FailedReason.Should().Be("Failed?");
        }


        [Test]
        public async Task ActivateStopLoss_GivenCancelledStopLoss_ShouldCancelTheStopLoss()
        {
            // arrange
            Setup();
            var (dateTime, quantityBought, addTrade) = SetupSale();
            var stopLossAmount = _data.ActiveTrade().BuyPrice * 0.9m;
            await _fakeStrategy.SetStopLoss(_data, stopLossAmount);
            var validStopLoss = _data.ActiveTrade().GetValidStopLoss();
            var activeTrade = _data.ActiveTrade();
            var orderStatusById = new OrderHistorySummaryResponse { OrderStatusType = "Cancelled"};
            // action
            await BrokerUtils.ApplyOrderResultToStrategy(_data, _data.ActiveTrade(), validStopLoss, orderStatusById);
            // assert
            validStopLoss.OrderStatusType.Should().Be(OrderStatusTypes.Cancelled);
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
            await BrokerUtils.ApplyOrderResultToStrategy(_data, _data.ActiveTrade(), validStopLoss, _fakeBroker.BuyFeePercent);
            // assert
            trade.IsActive.Should().BeFalse();
            trade.EndDate.Should().BeCloseTo(DateTime.Now,2000);
            trade.SellValue.Should().Be(88.64m);
            trade.SellPrice.Should().BeApproximately(90000, 1m);
            trade.Profit.Should().Be(-11.360m);
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
            last.StopPrice.Should().Be(90090m);
            last.RemainingQuantity.Should().Be(0);
            last.OriginalQuantity.Should().Be(0);
            last.OrderSide.Should().Be(0);
            last.OrderType.Should().Be(StrategyTrade.OrderTypeStopLoss);
            last.BrokerOrderId.Should().EndWith("-req");
            last.FailedReason.Should().Be(null);
            last.PriceAtRequest.Should().Be(100000M);
            last.Total.Should().Be(0.001m);
            last.TotalFee.Should().Be(0);
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
                OriginalQuantity = 0.00099990M,
                OrderSide = Side.Buy,
                OrderType = "simple",
                BrokerOrderId = trade.Orders.First().Id + "_broker",
                FailedReason = null,
                PriceAtRequest = 100000,
                Total = buyValue,
                FeeCurrency = "BTC",
                TotalFee = 0.000007499250M,
            };
            // assert
            
            trade.Orders.First().Should().BeEquivalentTo(expectedOrder,
                e => e
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));
           
        }

        [Test]
        public async Task Buy_Given100Rand_ShouldUpdateTheStrategyInstanceTo()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;

            SetupTrade(close, buyValue);
            // action
            await _fakeStrategy.Buy(_data, buyValue);
            // assert
            _data.StrategyInstance.InvestmentAmount.Should().Be(1000);
            _data.StrategyInstance.QuoteAmount.Should().Be(900);
            _data.StrategyInstance.BaseAmount.Should().Be(_data.StrategyInstance.Trades[0].BuyQuantity);
            _data.StrategyInstance.BaseAmount.Should().Be(0.000992400750M);
        }

        [Test]
        public async Task Sell_Given100Rand_ShouldUpdateTheStrategyInstanceTo()
        {
            // arrange
            Setup();
            var close = 100000;
            var buyValue = 100m;

            SetupTrade(close, buyValue);
            await _fakeStrategy.Buy(_data, buyValue);
            // action
            await _fakeStrategy.Sell(_data, _data.ActiveTrade());
            // assert
            _data.StrategyInstance.InvestmentAmount.Should().Be(1000);
            _data.StrategyInstance.QuoteAmount.Should().Be(998.59m);
            _data.StrategyInstance.BaseAmount.Should().Be(0.00000000m);
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
                OriginalQuantity = quantityBought,
                OrderSide = Side.Sell,
                OrderType = "simple",
                FailedReason = null,
                PriceAtRequest = 100000,
                Total = 100.10M,
                FeeCurrency = "ZAR",
                TotalFee = 0.75M,
            };
            // action
            await _fakeStrategy.Sell(_data, addTrade);
            // assert
            _data.StrategyInstance.Print();
            addTrade.Orders.Last().Dump("last").Should().BeEquivalentTo(expectedOrder,
                e => e
                    .Excluding(x => x.Id)
                    .Excluding(x => x.BrokerOrderId)
                    .Excluding(x => x.TotalFee)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));
            addTrade.Orders.Last().TotalFee.Should().BeApproximately(expectedOrder.TotalFee, 0.1m);
        }

 

        private StrategyTrade SetupTrade(int close, decimal buyValue)
        {
            var tradeFeedCandle = AddCandle(close, _requestDate);
            var expectedTrade = new StrategyTrade(tradeFeedCandle.Date, close, buyValue / close, buyValue);
            return expectedTrade;
        }

        private TradeQuote AddCandle(int close, DateTime dateTime)
        {
            var tradeFeedCandle = Builder<TradeQuote>.CreateNew().WithValidData()
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