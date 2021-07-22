using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Core.Utils
{
    public static class Retry
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public static async Task Run(Func<Task> action, CancellationToken token = default, int delaySeconds = 1, int maxDelay = 500)
        {
            var seconds = delaySeconds;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await action();
                    break;
                }
                catch (ApiResponseException e)
                {
                    var fromSeconds = TimeSpan.FromSeconds(Math.Min(seconds, 60));
                    await LogAndDelay(token, e, fromSeconds);
                }
                catch (Exception e)
                {
                    var fromSeconds = TimeSpan.FromSeconds(seconds);
                    await LogAndDelay(token, e, fromSeconds);
                }

                seconds = Math.Min(seconds * 2, maxDelay);
            }
        }

        private static async Task LogAndDelay(CancellationToken token, Exception e, TimeSpan fromSeconds)
        {
            _log.Warning(
                $"Action failed with {e.GetType().Namespace} {e.Message}. Trying again in {fromSeconds.ToShort()}");
            await Task.Delay(fromSeconds, token);
        }
    }
}