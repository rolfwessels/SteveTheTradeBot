using System;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrBrokerPaperTradingApi : IBrokerApi
    {
        private ValrBrokerApi _valrBrokerApi;
        private IMessenger _messenger;

        public ValrBrokerPaperTradingApi(string apiKey, string secret, IMessenger messenger)
        {
            _valrBrokerApi = new ValrBrokerApi( apiKey,  secret);
            _messenger = messenger;
        }

        #region Implementation of IBrokerApi

        public async Task<OrderHistorySummaryResponse> MarketOrder(SimpleOrderRequest request)
        {
            var quoteResponse = await _valrBrokerApi.Quote(request); 
            return new OrderHistorySummaryResponse
            {
                OrderId = quoteResponse.Id,
                OrderStatusType = "Filled",
                CustomerOrderId = request.CustomerOrderId,
                CurrencyPair = request.CurrencyPair,
                AveragePrice = quoteResponse.OrdersToMatch.Average(x=>x.Price),

                OriginalPrice = quoteResponse.PayAmount,
                Total = request.Side == Side.Buy ? quoteResponse.PayAmount : quoteResponse.ReceiveAmount,
                OriginalQuantity = request.Side == Side.Buy ? quoteResponse.ReceiveAmount : quoteResponse.PayAmount,

                FeeCurrency = quoteResponse.FeeCurrency,
                TotalFee = quoteResponse.Fee,
                OrderType = "simple",
                FailedReason = "",
                OrderUpdatedAt = quoteResponse.CreatedAt,
            };
        }

        public static OrderHistorySummaryResponse ToHistorySummary(SimpleOrderStatusResponse simpleResponse, SimpleOrderRequest request)
        {
            return new OrderHistorySummaryResponse
            {
                OrderId = simpleResponse.OrderId,
                OrderStatusType = simpleResponse.Success ? "Filled" : "Failed",
                CustomerOrderId = simpleResponse.OrderId,
                CurrencyPair = request.CurrencyPair,
                AveragePrice = simpleResponse.OriginalPrice(request.Side),
                OriginalPrice = simpleResponse.PaidAmount,
                Total = request.Side == Side.Buy? simpleResponse.PaidAmount : simpleResponse.ReceivedAmount,
                OriginalQuantity = request.Side == Side.Buy ? simpleResponse.ReceivedAmount: simpleResponse.PaidAmount,
                FeeCurrency = simpleResponse.ReceivedCurrency,
                TotalFee = simpleResponse.FeeAmount,
                OrderType = "simple",
                FailedReason = simpleResponse.FailedReason,
                OrderUpdatedAt = simpleResponse.OrderExecutedAt,
            };
        }

        public async Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request)
        {
            await Task.Delay(1000);
            return new IdResponse() {  Id = Guid.NewGuid().ToString( )};
        }

        public async Task<SimpleOrderStatusResponse> Order(SimpleOrderRequest simpleOrderRequest)
        {
            //await _valrBrokerApi.OrderBook(simpleOrderRequest.CurrencyPair);
            var quoteResponse = await _valrBrokerApi.Quote(simpleOrderRequest);;
            return new SimpleOrderStatusResponse()
            {
                OrderId = quoteResponse.Id,
                Success = true,
                Processing = false,
                PaidAmount = quoteResponse.PayAmount,
                PaidCurrency = quoteResponse.CurrencyPair.SideOut(simpleOrderRequest.Side),
                ReceivedAmount = quoteResponse.ReceiveAmount,
                ReceivedCurrency = quoteResponse.CurrencyPair.SideIn(simpleOrderRequest.Side),
                FeeAmount = quoteResponse.Fee,
                FeeCurrency = quoteResponse.FeeCurrency,
                OrderExecutedAt = quoteResponse.CreatedAt,
            };
        }

        public async Task CancelOrder(string brokerOrderId, string pair)
        {
            await Task.Delay(1000);
        }

        public async Task<bool> SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext)
        {
            var activeTrades = instance.ActiveTrade();
            var validStopLoss = activeTrades?.GetValidStopLoss();
            if (validStopLoss != null && strategyContext.LatestQuote().Low < validStopLoss.OrderPrice)
            {
                await BrokerUtils.ApplyOrderResultToStrategy(strategyContext, activeTrades, validStopLoss, 0.001m);
                return true;
            }

            return false;
        }


        #endregion
    }
}