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

        public static  Task Run(Func<Task> action, CancellationToken token = default,
            int delaySeconds = 1,
            int maxDelay = 500)
        {
            return Run<bool>(async () =>
            {
                await action();
                return true;
            }, token, delaySeconds, maxDelay);
        }

        public static async Task<T> Run<T>(Func<Task<T>> action, CancellationToken token = default, int delaySeconds = 1,
            int maxDelay = 500)
        {
            var seconds = delaySeconds;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    return await action();
                }

                catch (Exception e)
                {
                    var fromSeconds = TimeSpan.FromSeconds(seconds);
                    if (e.Message.Contains("TooManyRequests"))
                    {
                        var oneMinFromNow = DateTime.Now.AddMinutes(1).ToMinute().AddSeconds(5).TimeTill();
                        fromSeconds = TimeSpan.FromSeconds(Math.Max(seconds, oneMinFromNow.TotalSeconds));
                    }
                    await LogAndDelay(token, e, fromSeconds);
                }

                seconds = Math.Min(seconds * 2, maxDelay);
            }

            return default;
        }

        private static async Task LogAndDelay(CancellationToken token, Exception e, TimeSpan fromSeconds)
        {
            if (fromSeconds > TimeSpan.FromSeconds(59))
            {
                _log.Warning(
                    $"Action failed with {e.GetType().Name} {e.Message}. Trying again in {fromSeconds.ToShort()}");
            }
            else
            {
                _log.Debug(
                    $"[WRN] Action failed with {e.GetType().Name} {e.Message}. Trying again in {fromSeconds.ToShort()}");
            }
            await Task.Delay(fromSeconds, token);
        }
    }
}