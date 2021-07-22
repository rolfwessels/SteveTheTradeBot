using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Api
{
    public class TickerTrackerService : BackgroundService
    {
        public static bool IsFirstRunDone { get; private set; }
        private readonly IUpdateHistoricalData _historicalData;
        public TickerTrackerService(IUpdateHistoricalData historicalData)
        {
            _historicalData = historicalData;
        }

        #region Implementation of IHostedService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var enumerable = ValrFeeds.All.Select(x=> RunWithRetry(() => _historicalData.PopulateNewData(x.CurrencyPair, token), token));
                await Task.WhenAll(enumerable);
                IsFirstRunDone = true;
                await Task.Delay(TimeSpan.FromSeconds(30), token);
            }
        }


        #endregion
    }
}