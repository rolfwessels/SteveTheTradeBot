using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Bots;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{

    public class BackTestRunnerTests
    {
        private BackTestRunner _backTestRunner;

        [Test]
        public async Task Run_GivenSmallAmountOfData_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            _backTestRunner = new BackTestRunner(new DynamicGraphs(TestTradePersistenceFactory.InMemoryDb));
            var list =  Builder<HistoricalTrade>.CreateListOfSize(500).WithValidData().Build();
            // action
            var backTestResult = await _backTestRunner.Run(list.ToCandleOneMinute().Aggregate(PeriodSize.OneMinute),new RSiBot(new FakeBroker(null)), CancellationToken.None, CurrencyPair.BTCZAR);
            // assert
            backTestResult.TradesActive.Should().Be(0);
        }



        [Test]
        [Timeout(240000)]
        public async Task Run_Given_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            var factory = TestTradePersistenceFactory.RealDb();
            var tradeHistoryStore = new TradeHistoryStore(factory);
            var historicalDataPlayer = new HistoricalDataPlayer(tradeHistoryStore);
            _backTestRunner = new BackTestRunner(new DynamicGraphs(factory));
            var cancellationTokenSource = new CancellationTokenSource();
            var from = DateTime.Parse("2020-11-01   T00:00:00");
            var to = from.AddDays(20);
            var readHistoricalTrades = historicalDataPlayer.ReadHistoricalData(from, to,PeriodSize.FiveMinutes, cancellationTokenSource.Token);
            // action
            var backTestResult = await _backTestRunner.Run(readHistoricalTrades, new RSiBot(new FakeBroker(tradeHistoryStore)), CancellationToken.None, CurrencyPair.BTCZAR);
            // assert
            backTestResult.Dump("").BalanceMoved.Should().BeGreaterThan(backTestResult.MarketMoved);
        }

        #region Setup/Teardown

        public void Setup()
        {
            TestLoggingHelper.EnsureExists();
            
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion
    }

    public class FakeBroker : IBrokerApi
    {
        public decimal BuyFeePercent { get; set; } = 0.001m; // 0.0075m;
        public decimal AskPrice { get; set; } = 100010;
        public int BidPrice { get; set; } = 100100;
        private readonly ITradeHistoryStore _tradeHistoryStore;
        public List<object> Requests { get; }
        
        public FakeBroker(ITradeHistoryStore tradeHistoryStore = null)
        {
            _tradeHistoryStore = tradeHistoryStore;
            Requests = new List<object>();
        }

        #region Implementation of IBrokerApi

        public async Task<SimpleOrderStatusResponse> Order(SimpleOrderRequest request)
        {

            Requests.Add(request);
            var price = await GetAskPrice(request.RequestDate, request.Side);
            var totalAmount = Math.Round(request.PayAmount/ price,8);
            var feeAmount = Math.Round(totalAmount * BuyFeePercent, 12);
            var receivedAmount = Math.Round(totalAmount - feeAmount, 8);
            if (request.Side == Side.Sell)
            {
                totalAmount = Math.Round(request.PayAmount * price, 2);
                feeAmount = Math.Round(totalAmount * BuyFeePercent, 2);
                receivedAmount = Math.Round(totalAmount - feeAmount, 2);
            }

            
            return new SimpleOrderStatusResponse()
            {
                OrderId = request.CustomerOrderId + "_broker",
                Success = true,
                Processing = false,
                PaidAmount = request.PayAmount,
                PaidCurrency = request.PayInCurrency,
                ReceivedAmount = receivedAmount,
                ReceivedCurrency = request.CurrencyPair.SideIn(request.Side),
                FeeAmount = feeAmount,
                FeeCurrency = request.CurrencyPair.SideIn(request.Side),
                OrderExecutedAt = request.RequestDate.AddSeconds(1),
            };
        }

        private async Task<decimal> GetAskPrice(DateTime requestRequestDate, Side side)
        {
            decimal price = Side.Buy == side ? AskPrice : BidPrice;

            if (_tradeHistoryStore != null)
            {
                var findByDate = await _tradeHistoryStore.FindByDate(requestRequestDate,
                    requestRequestDate.Add(TimeSpan.FromMinutes(5)), 0, 1);
                price = findByDate.Select(x => x.Price).Last();
            }

            return price;
        }

        public async Task<OrderStatusResponse> LimitOrder(LimitOrderRequest request)
        {
            Requests.Add(request);
            
            return new OrderStatusResponse()
            {
                CustomerOrderId = request.CustomerOrderId,
                OrderStatusType = "Filled",
                CurrencyPair = request.Pair,
                OriginalPrice = await GetAskPrice(request.DateTime, Side.Buy),
                OrderSide = request.Side,
                RemainingQuantity = 0,
                OriginalQuantity = request.Quantity,
                OrderType = "market",
                OrderId = request.CustomerOrderId + "_broker"
            };
        }

        public async Task<OrderStatusResponse> MarketOrder(MarketOrderRequest request)
        {
            Requests.Add(request);
            return new OrderStatusResponse()
            {
                 CustomerOrderId = request.CustomerOrderId,
                 OrderStatusType = "Filled",
                 CurrencyPair = request.Pair,
                 OriginalPrice = await GetAskPrice(request.DateTime, Side.Buy),
                 OrderSide = request.Side,
                 RemainingQuantity = 0,
                 OriginalQuantity = request.Quantity,
                 OrderType = "market",
                 OrderId = request.CustomerOrderId+"_broker"
            };
        }

        public Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request)
        {
            throw new NotImplementedException();
        }

       

        #endregion
    }
}