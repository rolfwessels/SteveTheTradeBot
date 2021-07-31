using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public class StrategyService : BackgroundServiceWithResetAndRetry
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IStrategyInstanceStore _strategyStore;
        private readonly IMessenger _messenger;
        private readonly IStrategyRunner _runner;
        

        public StrategyService(IStrategyInstanceStore strategyStore, IMessenger messenger, IStrategyRunner runner)
        {
            _strategyStore = strategyStore;
            _messenger = messenger;
            _runner = runner;
        }

        #region Implementation of IHostedService

        protected override void RegisterSetter()
        {
            _messenger.Register<PopulateOtherMetrics.MetricsUpdatedMessage>(this, x => _delayWorker.Set());
        }

        protected override async Task ExecuteAsyncInRetry(CancellationToken token)
        {
            var found = await _strategyStore.FindActiveStrategies();
            var time = DateTime.Now.ToMinute();
            foreach (var strategyInstance in found)
            {
                if (token.IsCancellationRequested) return;
                try
                {
                    await _runner.Process(strategyInstance, time);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Error wile running {strategyInstance.Reference}:", e.Message);
                }

            }
        }

        #endregion
    }

   
    
}