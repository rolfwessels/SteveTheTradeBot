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
    public class StrategyService : BackgroundService
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IStrategyInstanceStore _strategyStore;
        private readonly IMessenger _messenger;
        private readonly IStrategyRunner _runner;
        readonly ManualResetEventSlim _delayWorker = new ManualResetEventSlim(false);
        

        public StrategyService(IStrategyInstanceStore strategyStore, IMessenger messenger, IStrategyRunner runner)
        {
            _strategyStore = strategyStore;
            _messenger = messenger;
            _runner = runner;
        }

        #region Implementation of IHostedService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            _messenger.Register<PopulateOtherMetrics.Updated>(this, x => _delayWorker.Set());
            await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
            while (!token.IsCancellationRequested)
            {
                _delayWorker.Wait(token);
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
                        _log.Error(e,$"Error wile running {strategyInstance.Reference}:", e.Message);
                    }
                    
                }
                
                await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
            }
        }

      

        #endregion
    }

   
    
}