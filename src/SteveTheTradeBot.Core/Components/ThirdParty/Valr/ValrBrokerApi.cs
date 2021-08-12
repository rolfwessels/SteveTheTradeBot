using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using RestSharp;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Framework.Subscriptions;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrBrokerApi : ApiBase, IBrokerApi
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _apiKey;
        private readonly string _secret;

        public ValrBrokerApi(string apiKey, string secret) : base("https://api.valr.com/v1/")
        {
            _apiKey = apiKey;
            _secret = secret;
        }

        #region Implementation of IBrokerApi

        public async Task<OrderBookResponse> OrderBook(string currencyPair)
        {
            _log.Information($"OrderBook {currencyPair}");
            var request = new RestRequest("marketdata/{currencyPair}/orderbook", DataFormat.Json) {Method = Method.GET};
            request.AddUrlSegment("currencyPair", currencyPair);
            return await ExecuteAsync<OrderBookResponse>(request);
        }

        public async Task<OrderHistorySummaryResponse> OrderHistorySummary(string customerOrderId)
        {
            _log.Information($"OrderHistorySummary customerOrderId {customerOrderId}");
            var request = new RestRequest("orders/history/summary/customerorderid/{customerOrderId}", DataFormat.Json)
                {Method = Method.GET};
            request.AddUrlSegment("customerOrderId", customerOrderId);
            return await ExecuteAsync<OrderHistorySummaryResponse>(request);
        }


        public async Task<OrderHistorySummaryResponse> OrderHistorySummaryById(string orderId)
        {
            
            var request = new RestRequest("orders/history/summary/orderid/{orderId}", DataFormat.Json)
                { Method = Method.GET };
            request.AddUrlSegment("orderId", orderId);
            return await ExecuteAsync<OrderHistorySummaryResponse>(request);
        }


        public async Task<List<OrderHistorySummaryResponse>> OrderHistory()
        {
            var request = new RestRequest("/orders/history", DataFormat.Json)
                { Method = Method.GET };
            request.AddParameter("skip", 0);
            request.AddParameter("limit",10);
            return await ExecuteAsync<List<OrderHistorySummaryResponse>>(request);
        }


        public async Task<IdResponse> SimpleOrder(SimpleOrderRequest simpleOrderRequest)
        {
            _log.Information(
                $"SimpleOrder {simpleOrderRequest.CurrencyPair} {simpleOrderRequest.PayAmount} {simpleOrderRequest.PayInCurrency}");
            var request = new RestRequest("/simple/{currencyPair}/order", DataFormat.Json) {Method = Method.POST};
            request.AddUrlSegment("currencyPair", simpleOrderRequest.CurrencyPair);
            request.AddJsonBody(simpleOrderRequest);
            return await ExecuteAsync<IdResponse>(request);
        }

        public async Task<SimpleOrderStatusResponse> SimpleOrderStatus(string currencyPair, string orderId)
        {
            _log.Information($"OrderBook {currencyPair}");
            var request = new RestRequest("/simple/{currencyPair}/order/{id}", DataFormat.Json) {Method = Method.GET};
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("id", orderId);
            return await ExecuteAsync<SimpleOrderStatusResponse>(request);
        }

        public async Task<IdResponse> StopLimitOrder(StopLimitOrderRequest stopLimitOrderRequest)
        {
            var request = new RestRequest("/orders/stop/limit", DataFormat.Json) {Method = Method.POST};
            request.AddJsonBody(AsRequest(stopLimitOrderRequest));
            return await ExecuteAsync<IdResponse>(request);
        }


        public async Task<SimpleOrderStatusResponse> Order(SimpleOrderRequest simpleOrderRequest)
        {
            var simpleOrder = await SimpleOrder(simpleOrderRequest);
            return await SimpleOrderStatus(simpleOrderRequest.CurrencyPair, simpleOrder.Id);
        }

        public async Task CancelOrder(string brokerOrderId, string pair)
        {
            var request = new RestRequest("/orders/order", DataFormat.Json) { Method = Method.DELETE };
            request.AddJsonBody(
                new
                {
                    OrderId = brokerOrderId,
                    Pair = pair
                });
            await ExecuteAsync<IdResponse>(request);
        }

        public async Task<bool> SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext)
        {
            var activeTrades = instance.ActiveTrade();
            var validStopLoss = activeTrades?.GetValidStopLoss();
            if (validStopLoss != null )
            {
                var orderStatusResponse = await OrderStatus(instance.Pair, validStopLoss.Id);
                if (orderStatusResponse.OrderStatusType == "Active")
                {
                    return false;
                }
                var orderStatusById = await OrderHistorySummary(validStopLoss.Id);
                await BrokerUtils.ApplyOrderResultToStrategy(strategyContext, activeTrades, validStopLoss, orderStatusById);
                _log.Debug($"ValrBrokerApi:SyncOrderStatus {instance.Id} {instance.Name} stop loss status changed to {validStopLoss.OrderStatusType}!");    
                if (validStopLoss.OrderStatusType != OrderStatusTypes.Filled)
                {
                    await strategyContext.Messenger.Send(PostSlackMessage.From($"{instance.Name} had a stop loss which now has status of `{validStopLoss.OrderStatusType}`"));
                }
                return true;
            }

            return false;

        }

        public async Task<OrderHistorySummaryResponse> MarketOrder(SimpleOrderRequest request)
        {
            var orderExists = await OrderExists(request.CurrencyPair, request.CustomerOrderId);
            if (!orderExists)
            {
                await MarketOrderInternal(ToMarketOrder(request));
            }

            //var limitOrderRequest = new IdResponse {Id = "883b35dc-263f-4252-9222-41012ce7bfb8"};
            return await OrderHistorySummary(request.CustomerOrderId);
            
        }

        private async Task<IdResponse> MarketOrderInternal(MarketOrderRequest toMarketOrder)
        {

            var request = new RestRequest("/orders/market", DataFormat.Json) {Method = Method.POST};
            request.AddJsonBody(toMarketOrder);
            var quoteResponse = await ExecuteAsync<IdResponse>(request);
            return quoteResponse;
        }

        public MarketOrderRequest ToMarketOrder(SimpleOrderRequest request)
        {
            var isBase = request.CurrencyPair.BaseCurrency() == request.PayInCurrency;

            decimal? baseAmount = null;
            decimal? quoteAmount = null;
            if (isBase)
            {
                baseAmount = request.PayAmount;
            }
            else
            {
                quoteAmount = request.PayAmount;
            }

            return new MarketOrderRequest(request.Side, quoteAmount, baseAmount, request.CurrencyPair,
                request.CustomerOrderId, request.RequestDate);
        }

        public async Task<LimitOrderRequest> BuildLimitOrderRequest(SimpleOrderRequest request)
        {
            var orderBookResponse = await OrderBook(request.CurrencyPair);
            var remaining = request.PayAmount;

            var placeOrderFor = orderBookResponse.Asks
                .OrderBy(x => x.Price)
                .Where(x => remaining >= 0)
                .ForAll(x => remaining = (remaining - x.Value))
                .ToList().Dump("Values");
            remaining.Dump("remaining");

            var price = placeOrderFor.Average(x => x.Price);
            var quantity = Math.Round(request.PayAmount / price, 8);
            var limitOrderRequest = new LimitOrderRequest(request.Side, quantity, price, request.CurrencyPair,
                request.CustomerOrderId, request.RequestDate, true);
            return limitOrderRequest;
        }

        public async Task<QuoteResponse> Quote(SimpleOrderRequest simpleOrderRequest)
        {
            _log.Information(
                $"SimpleOrderQuote {simpleOrderRequest.CurrencyPair} {simpleOrderRequest.PayAmount} {simpleOrderRequest.PayInCurrency}");
            var request = new RestRequest("/simple/{currencyPair}/quote", DataFormat.Json) {Method = Method.POST};
            request.AddUrlSegment("currencyPair", simpleOrderRequest.CurrencyPair);
            request.AddJsonBody(AsRequest(simpleOrderRequest));
            var quoteResponse = await ExecuteAsync<QuoteResponse>(request);
            HackFixCurrency(quoteResponse);
            return quoteResponse;
        }

        private static void HackFixCurrency(QuoteResponse quoteResponse)
        {
            if (quoteResponse.FeeCurrency == "R") quoteResponse.FeeCurrency = "ZAR";
        }

        private object AsRequest(SimpleOrderRequest simpleOrderRequest)
        {
            return new
            {
                Side = simpleOrderRequest.Side.ToString().ToUpper(),
                PayInCurrency = simpleOrderRequest.PayInCurrency,
                PayAmount = simpleOrderRequest.PayAmount
            };
        }

        private object AsRequest(StopLimitOrderRequest request)
        {
            var requestTimeInForce = request.TimeInForce switch
            {
                TimeEnforce.GoodTillCancelled => "GTC",
                TimeEnforce.FillOrKill => "FOK",
                TimeEnforce.ImmediateOrCancel => "IOC",
                _ => throw new ArgumentOutOfRangeException()
            };

            var requestType = request.Type switch
            {
                StopLimitOrderRequest.Types.TakeProfitLimit => "TAKE_PROFIT_LIMIT",
                StopLimitOrderRequest.Types.StopLossLimit => "STOP_LOSS_LIMIT",
                _ => throw new ArgumentOutOfRangeException()
            };

            return new {
                     Side = request.Side,
                     Quantity = request.Quantity,
                     Price = request.Price,
                     Pair = request.Pair,
                     CustomerOrderId = request.CustomerOrderId,
                     TimeInForce = requestTimeInForce,
                     StopPrice = request.StopPrice,
                     Type = requestType,
                };
        }


        public async Task<OrderStatusResponse> OrderStatusById(string currencyPair,string orderId)
        {
            var request = new RestRequest("/orders/{currencyPair}/orderid/{orderId}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("orderId", orderId);
            return await ExecuteAsync<OrderStatusResponse>(request);
        }

        public async Task<OrderStatusResponse> OrderStatus(string currencyPair,string customerOrderId)
        {
            var request = new RestRequest("/orders/{currencyPair}/customerorderid/{customerOrderId}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("customerOrderId", customerOrderId);
            return await ExecuteAsync<OrderStatusResponse>(request);
        }

        public async Task<bool> OrderExists(string currencyPair, string customerOrderId)
        {
            var request = new RestRequest("/orders/{currencyPair}/customerorderid/{customerOrderId}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("customerOrderId", customerOrderId);
            AddAuth(request);
            var executeAsync = await _client.ExecuteAsync(request);
            return executeAsync.StatusCode != (HttpStatusCode) 400;
        }

        public async Task<OrderStatusResponse> OrderStatusByOrderId(string currencyPair, string orderId)
        {
            var request = new RestRequest("/orders/{currencyPair}/orderid/{orderId}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("orderId", orderId);
            return await ExecuteAsync<OrderStatusResponse>(request);
        }



        #endregion

        #region Private Methods


        private void AddAuth(RestRequest request)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            request.AddHeader("X-VALR-API-KEY", _apiKey);
            request.AddHeader("X-VALR-SIGNATURE", SignBody(timestamp, request.Method.ToString(), _client.BuildUri(request).PathAndQuery,GetBody(request)));
            request.AddHeader("X-VALR-TIMESTAMP", timestamp);
        }

        protected async Task<T> ExecuteAsync<T>(RestRequest request)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            IRestResponse<T> response = null;
            var url = _client.BuildUri(request).PathAndQuery;
            try
            {
                AddAuth(request);

                response = await _client.ExecuteAsync<T>(request);
                var validateResponse = ValidateResponse(response);
                stopwatch.Stop();
                _log.Information($"Valid request [{stopwatch.Elapsed.ToShort()}] to {request.Method}:{url}");
                _log.Debug($"Request {request.Method}:{url} `{GetBody(request)}`");
                if (response.RawBytes.Length > 0)
                    _log.Debug($"Response {request.Method}:{url} `{Encoding.UTF8.GetString(response.RawBytes)}`");
                return validateResponse;
            }
            catch (Exception e)
            {
                _log.Warning($"Failed request [{stopwatch.Elapsed.ToShort()}] to {request.Method}:{url} {e.Message}");
                _log.Debug($"Request {request.Method}:{url} {GetBody(request)}");
                if (response?.RawBytes.Length > 0)
                    _log.Debug($"Response {request.Method}:{url} `{Encoding.UTF8.GetString(response.RawBytes)}`");
                throw;
            }
        }

        public string SignBody(string timestamp, string verb, string path, string body = null)
        {
            var payload = timestamp + verb.ToUpper() + path + body;
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return hash.ToHexString();
        }

        #endregion

       
    }
}