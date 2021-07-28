using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public class TickerTrackerService : BackgroundService
    {
        private readonly IUpdateHistoricalData _historicalData;
        private readonly IMessenger _messenger;

        public TickerTrackerService(IUpdateHistoricalData historicalData, IMessenger messenger)
        {
            _historicalData = historicalData;
            _messenger = messenger;
        }

        #region Implementation of IHostedService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var enumerable = ValrFeeds.All.Select(x=> RunWithRetry(() => _historicalData.PopulateNewData(x.CurrencyPair, token), token));
                await Task.WhenAll(enumerable);
                await _messenger.Send(new TickerUpdatedMessage());
                await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
            }
        }

        public class TickerUpdatedMessage
        {
        }

        #endregion
    }
}