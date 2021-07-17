using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Cmd
{
    public class DataCommand
    {
        public class Download : Command<Download.Settings>
        {
            private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
            private readonly TimeSpan _retryIn = TimeSpan.FromMinutes(1);

            public sealed class Settings : BaseCommandSettings
            {
            }

            #region Overrides of Command<Settings>

            public override int Execute(CommandContext context, Settings settings)
            {
                var cancellationTokenSource = BindToCancelKey();

                foreach (var feed in ValrFeeds.All)
                {
                    AnsiConsole.MarkupLine($"Start processing {feed.CurrencyPair}!.");
                    // ReSharper disable once MethodSupportsCancellation
                    Process(feed, cancellationTokenSource.Token).Wait();
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    AnsiConsole.MarkupLine($"Done {feed.CurrencyPair}!.");
                }

                if (cancellationTokenSource.IsCancellationRequested) AnsiConsole.MarkupLine("Stopped");
                return 1;
            }

            private static CancellationTokenSource BindToCancelKey()
            {
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    AnsiConsole.MarkupLine("Stopping...");
                    cancellationTokenSource.Cancel(false);
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
                };
                return cancellationTokenSource;
            }

            public async Task Process(ValrFeeds.Feed feed, CancellationToken token)
            {
                var historicalDataApi = IocApi.Instance.Resolve<IUpdateHistoricalData>();
                while (!token.IsCancellationRequested) 
                {
                    try
                    {
                        if (token.IsCancellationRequested) return;
                        await historicalDataApi.UpdateHistory(feed.CurrencyPair, token);
                        break;
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                        AnsiConsole.MarkupLine($"Trying again in {_retryIn.ToShort()}!.");
                        await Task.Delay(_retryIn, token);
                    }
                }
                
            }

            #endregion
        }
    }
}