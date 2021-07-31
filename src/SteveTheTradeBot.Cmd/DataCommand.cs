using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Cmd
{
    public class DataCommand
    {
        
        public class Download : CommandSync<Download.Settings>
        {
            private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
            private readonly TimeSpan _retryIn = TimeSpan.FromMinutes(1);

            public sealed class Settings : BaseCommandSettings
            {
            }

            #region Overrides of Command<Settings>

            public override async Task ExecuteAsync(Settings settings, CancellationToken token)
            {
                foreach (var feed in ValrFeeds.All)
                {
                    AnsiConsole.MarkupLine($"Start processing {feed.CurrencyPair}!.");
                    // ReSharper disable once MethodSupportsCancellation
                    await Process(feed, token);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    AnsiConsole.MarkupLine($"Done {feed.CurrencyPair}!.");
                }

                if (token.IsCancellationRequested) AnsiConsole.MarkupLine("Stopped");
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

        public class Export : CommandSync<BaseCommandSettings>
        {
            #region Overrides of CommandSync<BaseCommandSettings>

            public override async Task ExecuteAsync(BaseCommandSettings settings, CancellationToken token)
            {
                foreach (var x in ValrFeeds.All)
                {
                    var fileName = $"{x.Name}_{x.CurrencyPair}.csv".ToLower();
                    AnsiConsole.MarkupLine($"Exporting trades to csv [yellow]{fileName}[/].");
                    await using (var writer = new StreamWriter(fileName))
                    {
                        var count = 0;
                        var take = 10000;
                        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            var tradeHistoryStore = IocApi.Instance.Resolve<ITradeHistoryStore>();
                            var skip = 0;
                            
                            do
                            {
                                var findByDate = await tradeHistoryStore.FindByDate(x.CurrencyPair, DateTime.UtcNow.AddYears(-5),
                                    DateTime.UtcNow, skip, take);
                                csv.WriteRecords(findByDate);
                                count += findByDate.Count;
                                skip += take;
                                await csv.FlushAsync();
                                if (findByDate.Count == 0) break;
                            } while (!token.IsCancellationRequested);
                        }

                        AnsiConsole.MarkupLine(token.IsCancellationRequested
                            ? $"Partial export [yellow]{fileName}[/] {count} lines."
                            : $"Done exporting [green]{fileName}[/] {count} lines.");
                    }

                   
                }
            }

            #endregion
        }

        public class Reset : CommandSync<Reset.Settings>
        {
            public sealed class Settings : BaseCommandSettings
            {
                [CommandOption("--days")]
                [Description("How many days to go back into.")]
                public int Days { get; set; } = 0;

                [CommandOption("-t")]
                [Description("Reset histotrical trades.")]
                public bool ResetHistoricalTrades { get; set; } = false;

                [CommandOption("-c")]
                [Description("Reset histotrical candles.")]
                public bool ResetHistoricalCandles { get; set; } = false;

                [CommandOption("-m")]
                [Description("Reset histotrical metrics.")]
                public bool ResetHistoricalMetrics { get; set; } = false;
            }

            #region Overrides of Command<Settings>
            
            public override async Task ExecuteAsync(Settings settings, CancellationToken token)
            {
                
                var persistence = await IocApi.Instance.Resolve<ITradePersistenceFactory>().GetTradePersistence();
                var dateTime = DateTime.Now.Date.AddDays(-settings.Days);
                AnsiConsole.MarkupLine($"Resetting date to be re-processed after [yellow]{dateTime}[/].");
                if (!settings.ResetHistoricalTrades && !settings.ResetHistoricalCandles &&
                    !settings.ResetHistoricalMetrics)
                {
                    AnsiConsole.MarkupLine($"Please select something to reset [yellow]add --help to see options[/].");
                }

                if (settings.ResetHistoricalTrades)
                {
                    AnsiConsole.MarkupLine($"Resetting [yellow]historical trades[/].");
                    persistence.HistoricalTrades.RemoveRange(persistence.HistoricalTrades.AsQueryable()
                        .Where(x => x.TradedAt > dateTime));
                }

                if (settings.ResetHistoricalCandles)
                {
                    AnsiConsole.MarkupLine($"Resetting [yellow]historical candles[/].");
                    persistence.TradeFeedCandles.RemoveRange(persistence.TradeFeedCandles.AsQueryable()
                        .Where(x => x.Date > dateTime));
                }

                if (settings.ResetHistoricalMetrics)
                {
                    AnsiConsole.MarkupLine($"Resetting [yellow]historical metrics[/].");
                    var simpleParams = persistence.SimpleParam.AsQueryable()
                        .Where(x => x.Key.StartsWith("metric_populate")).ToList();
                    simpleParams.ForEach(x => x.Value = dateTime.AddDays(-settings.Days).ToIsoDateString());
                }

                persistence.SaveChanges();
                AnsiConsole.MarkupLine($"[green]Done[/] 👍.");
            }

            #endregion
        }
    }
}