using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{
    public class FakeBroker : IBrokerApi
    {
        public decimal BuyFeePercent { get; set; } = 0.001m; // 0.0075m;
        public decimal AskPrice { get; set; } = 100010;
        public int BidPrice { get; set; } = 100100;
        private readonly ITradeHistoryStore _tradeHistoryStore;
        private Exception _exception;
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

        public Task CancelOrder(string brokerOrderId)
        {
            return Task.CompletedTask;
        }

        public Task SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext)
        {
            var activeTrades = instance.ActiveTrade();
            if (activeTrades != null)
            {
                var validStopLoss = activeTrades.GetValidStopLoss();
                if (strategyContext.LatestQuote().Low < validStopLoss.OrderPrice)
                {
                    ActivateStopLoss(strategyContext, activeTrades, validStopLoss);
                }
            }

            return Task.CompletedTask;
        }

        public void ActivateStopLoss(StrategyContext strategyContext, StrategyTrade activeTrades,
            TradeOrder validStopLoss)
        {
            var totalAmount = validStopLoss.OutQuantity * validStopLoss.PriceAtRequest;
            var feeAmount = Math.Round(totalAmount * BuyFeePercent, 2);
            var receivedAmount = Math.Round(totalAmount - feeAmount, 2);
            validStopLoss.OriginalQuantity = receivedAmount;
            validStopLoss.FeeAmount = feeAmount;
            activeTrades.FeeCurrency = validStopLoss.FeeCurrency;
            BrokerUtils.ActivateStopLoss(activeTrades, strategyContext.LatestQuote().Date, validStopLoss);
            BrokerUtils.ApplyCloseToStrategy(strategyContext, activeTrades);
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
            if (_exception != null) throw _exception;
            Requests.Add(request);
            return Task.FromResult(new IdResponse() { Id = request.CustomerOrderId+"-req"});
        }

       

        #endregion

        public void Throw(Exception exception)
        {
            _exception = exception;
        }
    }
}