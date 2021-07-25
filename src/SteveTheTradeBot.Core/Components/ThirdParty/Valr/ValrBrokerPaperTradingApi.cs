using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrBrokerPaperTradingApi : IBrokerApi
    {
        private ValrBrokerApi _valrBrokerApi;

        public ValrBrokerPaperTradingApi(string apiKey, string secret)
        {
            _valrBrokerApi = new ValrBrokerApi( apiKey,  secret);
        }

        #region Implementation of IBrokerApi

        public Task<OrderStatusResponse> LimitOrder(LimitOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderStatusResponse> MarketOrder(MarketOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request)
        {
            throw new NotImplementedException();
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

        public Task CancelOrder(string brokerOrderId)
        {
            throw new NotImplementedException();
        }

        public Task SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}