using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using SteveTheTradeBot.Api.WebApi.Exceptions;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        protected static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private Task _currentTask;
        private readonly CancellationTokenSource _tokenSource =
            new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken token);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _log.Information($"Starting {GetType().Name}.");
            _currentTask = ExecuteAsync(_tokenSource.Token);
            return _currentTask.IsCompleted ? _currentTask : Task.CompletedTask;
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
            var seconds = delaySeconds;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await action();
                    break;
                }
                catch (ApiException e ) 
                {
                    var fromSeconds = TimeSpan.FromSeconds(seconds = 60);
                    _log.Warning($"Action {action} failed with exception {e.Message}. Trying again in {fromSeconds.ToShort()}");
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
                catch (Exception e)
                {
                    var fromSeconds = TimeSpan.FromSeconds(seconds);
                    _log.Warning($"Action {action} failed with exception {e.Message}. Trying again in {fromSeconds.ToShort()}");
                    await Task.Delay(fromSeconds, token);
                }
                seconds = Math.Max(seconds * 2, maxDelay);
            }
            
            
        }
    }
}