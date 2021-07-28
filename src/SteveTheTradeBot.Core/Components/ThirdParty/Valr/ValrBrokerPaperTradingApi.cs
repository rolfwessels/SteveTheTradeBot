using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Strategies;
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

        public Task<OrderStatusResponse> LimitOrder(LimitOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderStatusResponse> MarketOrder(MarketOrderRequest request)
        {
            throw new NotImplementedException();
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

        public async Task CancelOrder(string brokerOrderId)
        {
            await Task.Delay(1000);
        }

        public Task SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext)
        {
            var activeTrades = instance.ActiveTrade();
            var validStopLoss = activeTrades?.GetValidStopLoss();
            if (validStopLoss != null && strategyContext.LatestQuote().Low < validStopLoss.OrderPrice)
            {
                BrokerUtils.ActivateStopLoss(strategyContext, activeTrades, validStopLoss, 0.001m);
                _messenger.Send(new TradeOrderMadeMessage(instance, activeTrades, validStopLoss));
            }
            return Task.CompletedTask;
        }

        #endregion
    }
}