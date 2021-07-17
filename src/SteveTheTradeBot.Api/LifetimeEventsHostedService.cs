using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SteveTheTradeBot.Api
{
    public class LifetimeEventsHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        #region Implementation of IHostedService

        public LifetimeEventsHostedService(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _log.Debug($"LifetimeEventsHostedService:StartAsync {cancellationToken.IsCancellationRequested}");
            return Task.CompletedTask;
        }

        private void OnStopping()
        {
            _log.Debug($"LifetimeEventsHostedService:OnStopping ");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.Debug($"LifetimeEventsHostedService:StopAsync {cancellationToken.IsCancellationRequested}");
            return Task.CompletedTask;
        }

        #endregion
    }
}