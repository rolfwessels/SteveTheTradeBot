using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class PopulateOtherCandlesService : BackgroundServiceWithResetAndRetry
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ITradeFeedCandlesStore _store;
        private readonly IMessenger _messenger;

        public PopulateOtherCandlesService(ITradeFeedCandlesStore store, IMessenger messenger)
        {
            _store = store;
            _messenger = messenger;
        }


        #region Overrides of BackgroundService


        protected override void RegisterSetter()
        {
            _messenger.Register<PopulateOneMinuteCandleService.OneMinuteCandleAvailable>(this, x => _delayWorker.Set());
        }

        protected override async Task ExecuteAsyncInRetry(CancellationToken token)
        {
            var tasks = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.OneMinute)
                .Select(x =>
                {
                    var (periodSize, feed) = x;
                    return _store.Populate(token, feed.CurrencyPair, feed.Name, periodSize);
                });
            await Task.WhenAll(tasks);
            await _messenger.Send(new UpdatedOtherCandles());
        }

       

        #endregion

        public class UpdatedOtherCandles
        {
            public UpdatedOtherCandles()
            {
            
            }
        }
    }

}