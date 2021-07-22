using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bumbershoot.Utilities.Helpers;
using Hangfire.Logging;
using MongoDB.Bson.IO;
using RestSharp.Serialization.Json;
using Serilog;
using Skender.Stock.Indicators;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Cmd
{
    public class DataImportCommand : Command<DataImportCommand.Settings>
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public sealed class Settings : BaseCommandSettings
        {
        }

        #region Overrides of Command<Settings>

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.Status()
                .Start("Processing...", ctx =>
                {
                    AnsiConsole.MarkupLine("Initialize db.");
                    ctx.Status("Initialize db");
                    var historicalDataPlayer = IocApi.Instance.Resolve<IHistoricalDataPlayer>();
                    var tradeHistoryStore = IocApi.Instance.Resolve<ITradePersistenceFactory>().GetTradePersistence()
                        .Result;
                    AnsiConsole.MarkupLine("Start reading records.");
                    var readHistoricalTrades = historicalDataPlayer.ReadHistoricalTrades(CurrencyPair.BTCZAR, DateTime.Now.AddYears(-10), DateTime.Now);
                    var counter = 0;

                    ctx.Status("Importing");
                    var quotes = readHistoricalTrades.ToCandleOneMinute()
                        .ForAll(x =>
                        {
                            counter++;
                            tradeHistoryStore.TradeFeedCandles.Add(
                                TradeFeedCandle.From(x, "valr", PeriodSize.OneMinute, "BTCZAR"));
                            if (counter % 1000 == 0)
                            {
                                tradeHistoryStore.SaveChanges();
                                //_log.Information($"Processed {counter} records up and till {x.Date}.");
                                AnsiConsole.MarkupLine($"Processed {counter} records up and till {x.Date}.");
                            }
                        })
                        .Aggregate(PeriodSize.FiveMinutes)
                        .ForAll(x =>
                            tradeHistoryStore.TradeFeedCandles.Add(TradeFeedCandle.From(x, "valr",
                                PeriodSize.FiveMinutes, "BTCZAR")))
                        .Aggregate(PeriodSize.FifteenMinutes)
                        .ForAll(x =>
                            tradeHistoryStore.TradeFeedCandles.Add(TradeFeedCandle.From(x, "valr",
                                PeriodSize.FifteenMinutes, "BTCZAR")))
                        .Aggregate(PeriodSize.ThirtyMinutes)
                        .ForAll(x =>
                            tradeHistoryStore.TradeFeedCandles.Add(TradeFeedCandle.From(x, "valr",
                                PeriodSize.ThirtyMinutes, "BTCZAR")))
                        .Aggregate(PeriodSize.OneHour)
                        .ForAll(x =>
                            tradeHistoryStore.TradeFeedCandles.Add(TradeFeedCandle.From(x, "valr", PeriodSize.OneHour, "BTCZAR")))
                        .Aggregate(PeriodSize.Day)
                        .ForAll(x =>
                            tradeHistoryStore.TradeFeedCandles.Add(TradeFeedCandle.From(x, "valr", PeriodSize.Day, "BTCZAR")))
                        .Aggregate(PeriodSize.Week)
                        .ForAll(x =>
                            tradeHistoryStore.TradeFeedCandles.Add(TradeFeedCandle.From(x, "valr", PeriodSize.Week, "BTCZAR")))
                        .ToList();
                    tradeHistoryStore.SaveChanges();
                    //_log.Information($"Processed {counter} records up and till {x.Date}.");
                    AnsiConsole.MarkupLine($"Done {counter} !.");
                });

            AnsiConsole.MarkupLine("Press any key to stop.");
            Console.In.Read();
            return 1;
        }

        #endregion
    }
}