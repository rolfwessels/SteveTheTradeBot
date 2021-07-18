using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Logging;
using Serilog;
using SteveTheTradeBot.Api.WebApi.Exceptions;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Api
{
    public class TickerTrackerService : BackgroundService
    {
        private readonly IUpdateHistoricalData _historicalData;
        private new static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public TickerTrackerService(IUpdateHistoricalData historicalData)
        {
            _historicalData = historicalData;
        }

        #region Implementation of IHostedService
        
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await RunWithRetry(() => ExecuteWithSync(token), token);
                await Task.Delay(TimeSpan.FromSeconds(30), token);
            }
        }

        private async Task ExecuteWithSync(CancellationToken token)
        {
            var tasks = ValrFeeds.All.Select(x=> _historicalData.PopulateNewData(x.CurrencyPair, token));
            await Task.WhenAll(tasks);
        }

        #endregion
    }
}