﻿using System;
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
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{

    [Category("Integration")]
    public class BackTestRunnerTests
    {
        private BackTestRunner _backTestRunner;



        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiBot_ShouldOver2YearsShouldMake400PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; // 209
            await Test(@from, to, expected, t => new RSiStrategy(t), CurrencyPair.BTCZAR);
        }

        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiBot2_ShouldOver1YearsShouldMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 95; // 209
            await Test(@from, to, expected , t => new NewRSiStrategy(t), CurrencyPair.BTCZAR);
        }

        [Test]
        [Timeout(240000)]
        [Ignore("Needs more work to get it working on ETH")]
        public async Task Run_GivenRSiBot2OnETHZAR_ShouldOver1YearsMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 95; // 209
            await Test(@from, to, expected , t => new NewRSiStrategy(t), CurrencyPair.ETHZAR);
        }

        private async Task Test(DateTime @from, DateTime to, int expected, Func<IBrokerApi,IStrategy> getStrategy, string currencyPair)
        {
            var factory = TestTradePersistenceFactory.RealDb();
            var tradeHistoryStore = new TradeHistoryStore(factory);
            var strategyInstanceStore = new StrategyInstanceStore(factory);
            var player = new HistoricalDataPlayer(tradeHistoryStore);
            
            var fakeBroker = new FakeBroker(tradeHistoryStore);
            var strategy = getStrategy(fakeBroker);
            var picker = new StrategyPicker().Add(strategy.Name, () => strategy);
            _backTestRunner = new BackTestRunner(new DynamicGraphs(factory), picker);
            var cancellationTokenSource = new CancellationTokenSource();

            var trades = player.ReadHistoricalData(currencyPair, @from, to, PeriodSize.FiveMinutes,
                cancellationTokenSource.Token);
            // action
            
            
            
            var strategyInstance = StrategyInstance.ForBackTest(strategy.Name, CurrencyPair.BTCZAR);
            await strategyInstanceStore.Add(strategyInstance);
            var backTestResult = await _backTestRunner.Run(strategyInstance, trades,  CancellationToken.None);
            // assert
           
            Console.Out.WriteLine("BalanceMoved: " + backTestResult.BalanceMoved);
            Console.Out.WriteLine("MarketMoved: " + backTestResult.MarketMoved);
            Console.Out.WriteLine("Trades: " + backTestResult.Trades.Count);
            Console.Out.WriteLine("TradesSuccesses: " + backTestResult.TradesSuccesses);
            Console.Out.WriteLine("TradesSuccessesPercent: " + backTestResult.TradesSuccessesPercent);
            Console.Out.WriteLine("TradesActive: " + backTestResult.TradesActive);
            Console.Out.WriteLine("AvgDuration: " + backTestResult.AvgDuration);
            var tradeValues = backTestResult.Trades
                .Select(x => new { x.StartDate, x.Profit, Value = x.Value - x.BuyValue, MarketMoved = TradeUtils.MovementPercent(x.SellPrice, x.BuyPrice) })
                .OrderByDescending(x => x.Value).ToArray();
            Console.Write(TradeUtils.ToTable(tradeValues.Take(10).Concat(tradeValues.TakeLast(10))).ToString());
            Console.Write(TradeUtils
                .ToTable(backTestResult.Trades.Select(x => new {x.StartDate, x.BuyValue, x.Quantity, x.BuyPrice, x.SellPrice}))
                .ToString());
            Console.Write(TradeUtils.ToTable(backTestResult.Trades.SelectMany(x => x.Orders).Select(x =>
                    new {x.OrderSide, x.PriceAtRequest, x.OrderPrice, x.OutQuantity, x.OriginalQuantity, x.CurrencyPair}))
                .ToString());

            backTestResult.BalanceMoved.Should().BeGreaterThan(expected);
            backTestResult.BalanceMoved.Should().BeGreaterThan(backTestResult.MarketMoved);
        }

        #region Setup/Teardown

        public void Setup()
        {
          //  TestLoggingHelper.EnsureExists();
            
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
            var price = await GetAskPrice(request.RequestDate, request.Side, request.CurrencyPair);
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

        private async Task<decimal> GetAskPrice(DateTime requestRequestDate, Side side, string currencyPair)
        {
            decimal price = Side.Buy == side ? AskPrice : BidPrice;

            if (_tradeHistoryStore != null)
            {
                var findByDate = await _tradeHistoryStore.FindByDate(currencyPair,requestRequestDate,
                    requestRequestDate.Add(TimeSpan.FromMinutes(5)), skip: 0, take: 1);
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
                OriginalPrice = await GetAskPrice(request.DateTime, Side.Buy, request.Pair),
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
                 OriginalPrice = await GetAskPrice(request.DateTime, Side.Buy, request.Pair),
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