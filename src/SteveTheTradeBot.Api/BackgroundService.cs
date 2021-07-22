using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Microsoft.Extensions.Hosting;
using Serilog;
using SteveTheTradeBot.Api.WebApi.Exceptions;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        protected static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private Task _currentTask;
        private readonly CancellationTokenSource _tokenSource =
            new CancellationTokenSource();

        public abstract Task ExecuteAsync(CancellationToken token);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _log.Information($"Starting {GetType().Name}.");
            _currentTask = ExecuteAsync(_tokenSource.Token);
            _currentTask.ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    LogException(x.Exception);
                }
            }, cancellationToken);
            return _currentTask.IsCompleted ? _currentTask : Task.CompletedTask;
        }

        private static void LogException(Exception aggregateException)
        {
            _log.Error($"{aggregateException.Message}\n {aggregateException.StackTrace}", aggregateException);
            if (aggregateException.InnerException != null)
            {
                LogException(aggregateException.InnerException);
            }
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_currentTask == null)
            {
                return;
            }

            try
            {
                _log.Information($"Stopping {GetType().Name}.");
                _tokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(_currentTask, Task.Delay(TimeSpan.FromSeconds(30), cancellationToken));
                _log.Information($"Stopped {GetType().Name}.");
            }

        }

        public virtual void Dispose()
        {
            _tokenSource.Cancel();
        }

        protected async Task RunWithRetry(Func<Task> action, CancellationToken token, int delaySeconds = 1, int maxDelay = 500)
        {
            await Retry.Run(action, token, delaySeconds, maxDelay);
        }
    }
}