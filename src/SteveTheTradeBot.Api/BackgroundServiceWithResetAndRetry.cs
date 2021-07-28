using System;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public abstract class BackgroundServiceWithResetAndRetry : BackgroundService
    {
        protected readonly ManualResetEventSlim _delayWorker = new ManualResetEventSlim(false);
        #region Overrides of BackgroundService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            RegisterSetter();
            while (!token.IsCancellationRequested)
            {
                await RunWithRetry(async () =>
                {
                    _delayWorker.Wait(token);
                    _delayWorker.Reset();
                    await ExecuteAsyncInRetry(token);
                    await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
                }, token, 30, 3000);
            }
        }
        protected abstract void RegisterSetter();
        protected abstract Task ExecuteAsyncInRetry(CancellationToken token);

        #endregion
    }
}