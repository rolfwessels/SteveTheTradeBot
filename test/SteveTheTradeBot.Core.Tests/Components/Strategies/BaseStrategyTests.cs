using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
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

        #region Setup/Teardown

        public void Setup()
        {
            _fakeBroker = new FakeBroker().With(x => x.BuyFeePercent = 0.0075m);
            _data = new StrategyContext(new DynamicGraphsTests.FakeGraph(), StrategyInstance.ForBackTest("BTCZAR", CurrencyPair.BTCZAR), _fakeBroker, Messenger.Default);
            var tradeFeedCandles = Builder<TradeFeedCandle>.CreateListOfSize(4).WithValidData().Build();
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
            trade.SellPrice.Should().BeApproximately(100100,1m);
            trade.Profit.Should().Be(-1.410m);
            trade.IsActive.Should().Be(false);
            trade.FeeAmount.Should().BeApproximately(1.49m,0.1m);
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

        private (DateTime dateTime, decimal quantityBought, StrategyTrade addTrade) SetupSale()
        {
            var close = 100000;
            var buyValue = 100m;
            SetupTrade(close, buyValue);
            var dateTime = DateTime.Parse("2001-01-01T01:02:03Z");
            var quantityBought = 0.1m;
            var addTrade = _data.StrategyInstance.AddTrade(dateTime, close, quantityBought, 2);
            return (dateTime, quantityBought, addTrade);
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
                RequestDate = new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc),
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
                RequestDate = new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc),
                OrderStatusType = OrderStatusTypes.Filled,
                CurrencyPair = "BTCZAR",
                OrderPrice = _fakeBroker.BidPrice,
                RemainingQuantity = 0,
                OriginalQuantity = 9934.92m,
                OrderSide = Side.Sell,
                OrderType = "simple",
                FailedReason = null,
                PriceAtRequest = 100000,
                OutQuantity = quantityBought,
                OutCurrency = "BTC",
                FeeCurrency = "ZAR",
                FeeAmount = 75M,
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
            var tradeFeedCandle = AddCandle(close, new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc));
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