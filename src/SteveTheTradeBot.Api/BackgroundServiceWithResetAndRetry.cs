using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Logging;
using Serilog;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public abstract class BackgroundServiceWithResetAndRetry : BackgroundService
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
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
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await ExecuteAsyncInRetry(token);
                    stopwatch.Stop();
                    _log.Debug($"{GetType().Name} service done processing in {stopwatch.Elapsed.ToShort()}");
                    await Task.Delay(DateTime.Now.AddMinutes(1).ToMinute().TimeTill(), token);
                }, token, 30, 3000);
            }
        }
        protected abstract void RegisterSetter();
        protected abstract Task ExecuteAsyncInRetry(CancellationToken token);

        #endregion
    }
}