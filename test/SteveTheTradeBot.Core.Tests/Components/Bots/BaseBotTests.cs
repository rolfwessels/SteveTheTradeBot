using System;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Bots;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Bots
{
    public class BaseBotTests
    {
        private FakeBot _fakeBot;
        private BackTestRunner.BotData _data;
        private FakeBroker _fakeBroker;

        #region Setup/Teardown

        public void Setup()
        {
            _data = new BackTestRunner.BotData(new DynamicGraphsTests.FakeGraph(), 1000, "name", CurrencyPair.BTCZAR);
            var tradeFeedCandles = Builder<TradeFeedCandle>.CreateListOfSize(4).WithValidData().Build();
            _data.ByMinute.AddRange(tradeFeedCandles);
            _fakeBroker = new FakeBroker().With(x=>x.BuyFeePercent = 0.0075m);
            
            _fakeBot = new FakeBot(_fakeBroker);
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
            var trade = await _fakeBot.Buy(_data, buyValue);
            // assert
            trade.Should().BeEquivalentTo(expectedTrade.Dump("expectedTrade"),
                e => e.Excluding(x => x.Orders)
                    .Excluding(x => x.Id)
                    .Excluding(x => x.FeeAmount)
                    .Excluding(x => x.Quantity)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));

            trade.Quantity.Should().BeApproximately(expectedTrade.Quantity, 0.01m);
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
            var trade = await _fakeBot.Buy(_data, buyValue);
            AddCandle(close - 5, new DateTime(2001, 01, 01, 3, 2, 4, DateTimeKind.Utc));

            // action
            await _fakeBot.Sell(_data, trade);
            // assert
            trade.Dump("asd");
            trade.EndDate.Should().Be(new DateTime(2001, 01, 01, 3, 2, 4, DateTimeKind.Utc));
            trade.Value.Should().Be(98.59m);
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
            var trade = await _fakeBot.Buy(_data, buyValue);
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
            await _fakeBot.Sell(_data, addTrade);
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

        private (DateTime dateTime, decimal quantityBought, Trade addTrade) SetupSale()
        {
            var close = 100000;
            var buyValue = 100m;
            SetupTrade(close, buyValue);
            var dateTime = DateTime.Parse("2001-01-01T01:02:03Z");
            var quantityBought = 0.1m;
            var addTrade = _data.BackTestResult.AddTrade(dateTime, close, quantityBought, 2);
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
            var trade = await _fakeBot.Buy(_data, buyValue);
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
            await _fakeBot.Sell(_data, addTrade);
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

        private Trade SetupTrade(int close, decimal buyValue)
        {
            var tradeFeedCandle = AddCandle(close, new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc));
            var expectedTrade = new Trade(tradeFeedCandle.Date, close, buyValue / close, buyValue);

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


    public class FakeBot : BaseBot
    {
        public FakeBot(IBrokerApi broker) : base(broker)
        {
        }

        #region Overrides of BaseBot

        public override Task DataReceived(BackTestRunner.BotData data)
        {
            throw new System.NotImplementedException();
        }

        public override string Name { get; } = "Test";

        #endregion
    }
}