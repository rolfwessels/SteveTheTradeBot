﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

            public override async Task ExecuteAsync(Settings settings)
            {
                var cancellationTokenSource = ConsoleHelper.BindToCancelKey();

                foreach (var feed in ValrFeeds.All)
                {
                    AnsiConsole.MarkupLine($"Start processing {feed.CurrencyPair}!.");
                    // ReSharper disable once MethodSupportsCancellation
                    await Process(feed, cancellationTokenSource.Token);
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    AnsiConsole.MarkupLine($"Done {feed.CurrencyPair}!.");
                }

                if (cancellationTokenSource.IsCancellationRequested) AnsiConsole.MarkupLine("Stopped");
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

        public class Reset : CommandSync<Reset.Settings>
        {
            private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

            public sealed class Settings : BaseCommandSettings
            {
                [CommandOption("--days")]
                [Description("How many days to go back into.")]
                public int Days { get; set; } = 5;
            }

            #region Overrides of Command<Settings>



            public override async Task ExecuteAsync(Settings settings)
            {
                
                var persistence = await IocApi.Instance.Resolve<ITradePersistenceFactory>().GetTradePersistence();
                var dateTime = DateTime.Now.Date.AddDays(-settings.Days);
                AnsiConsole.MarkupLine($"Resetting date to be re-processed after [yellow]{dateTime}[/].");
                persistence.HistoricalTrades.RemoveRange(persistence.HistoricalTrades.AsQueryable().Where(x => x.TradedAt > dateTime));
                persistence.TradeFeedCandles.RemoveRange(persistence.TradeFeedCandles.AsQueryable().Where(x => x.Date > dateTime));
                var simpleParams = persistence.SimpleParam.AsQueryable().Where(x => x.Key.StartsWith("metric_populate")).ToList();
                simpleParams.ForEach(x=>x.Value = dateTime.AddDays(-settings.Days).ToIsoDateString());
                persistence.SaveChanges();
            }

            #endregion
        }
    }
}