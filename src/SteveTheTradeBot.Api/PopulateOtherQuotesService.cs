using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;

namespace SteveTheTradeBot.Api
{
    public class PopulateOtherQuotesService : BackgroundServiceWithResetAndRetry
    {
        private readonly ITradeQuoteStore _store;
        private readonly IMessenger _messenger;

        public PopulateOtherQuotesService(ITradeQuoteStore store, IMessenger messenger)
        {
            _store = store;
            _messenger = messenger;
        }

        #region Overrides of BackgroundService

        protected override void RegisterSetter()
        {
            _messenger.Register<PopulateOneMinuteQuoteService.OneMinuteCandleAvailable>(this, x => _delayWorker.Set());
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
            await _messenger.Send(new UpdatedOtherQuotes());
        }

        #endregion

        public class UpdatedOtherQuotes
        {
        }
    }

}