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
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Bots
{
    public class BaseBotTests
    {
        private FakeBot _fakeBot;
        private BackTestRunner.BotData _data;
        private FakeBroker fakeBroker;

        #region Setup/Teardown

        public void Setup()
        {
            _data = new BackTestRunner.BotData(new DynamicGraphsTests.FakeGraph(), 1000, "name", CurrencyPair.BTCZAR);
            var tradeFeedCandles = Builder<TradeFeedCandle>.CreateListOfSize(4).WithValidData().Build();
            _data.ByMinute.AddRange(tradeFeedCandles);
            fakeBroker = new FakeBroker(null);
            _fakeBot = new FakeBot(fakeBroker);
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
            // action
            var trade = await _fakeBot.Buy(_data, buyValue);
            // assert
            trade.Should().BeEquivalentTo(expectedTrade,
                e => e.Excluding(x => x.Orders)
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));
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
            var marketOrderRequest = fakeBroker.Requests
                .OfType<MarketOrderRequest>().First();
            marketOrderRequest.Dump("asd");
            marketOrderRequest.Side.Should().Be(1);
            marketOrderRequest.Quantity.Should().Be(0.001m);
            marketOrderRequest.Pair.Should().Be("BTCZAR");
            marketOrderRequest.CustomerOrderId.Should().Be(trade.Orders.First().Id);
            marketOrderRequest.DateTime.Should().BeSameDateAs(DateTime.Parse("2001-01-01T01:02:03Z"));
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
                OriginalPrice = 100001,
                RemainingQuantity = 0,
                OriginalQuantity = 0.001m,
                OrderSide = Side.Buy,
                OrderType = "market",
                BrokerOrderId = trade.Orders.First().Id + "_broker",
                FailedReason = null,
                PriceAtRequest = 100000,
                OutQuantity = 100,
                OutCurrency = "ZAR",
            };
            // assert

            trade.Orders.First().Should().BeEquivalentTo(expectedOrder,
                e => e
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreateDate)
                    .Excluding(x => x.UpdateDate));
        }

        private Trade SetupTrade(int close, decimal buyValue)
        {
            var tradeFeedCandle = Builder<TradeFeedCandle>.CreateNew().WithValidData()
                .With(x => x.Close = close)
                .With(x => x.Date = new DateTime(2001, 01, 01, 1, 2, 3, DateTimeKind.Utc))
                .Build();
            _data.ByMinute.Add(tradeFeedCandle);
            var expectedTrade = new Trade(tradeFeedCandle.Date, close, buyValue / close, buyValue);

            return expectedTrade;
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