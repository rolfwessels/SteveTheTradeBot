using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Cmd
{
    public class StrategyCommand
    {
        public class Add : AsyncCommand<BaseCommandSettings>
        {
         

            #region Overrides of AsyncCommand<BaseCommandSettings>

            public override async Task<int> ExecuteAsync(CommandContext context, BaseCommandSettings settings)
            {
                var strategyStore = IocApi.Instance.Resolve<IStrategyInstanceStore>();
                var strategyNames = IocApi.Instance.Resolve<StrategyPicker>().List;

                var strategy = AnsiConsole.Prompt(new TextPrompt<string>("Pick a [green]strategy[/]?")
                    .InvalidChoiceMessage("[red]That's not a valid strategy[/]")
                    .DefaultValue(strategyNames.First())
                    .AddChoices(strategyNames));
                var pair = AnsiConsole.Prompt(new TextPrompt<string>("Pick a [green]pair[/]?")
                    .InvalidChoiceMessage("[red]That's not a valid pair[/]")
                    .DefaultValue(CurrencyPair.BTCZAR)
                    .AddChoice(CurrencyPair.BTCZAR)
                    .AddChoice(CurrencyPair.ETHZAR));
                var amount = AnsiConsole.Prompt(new TextPrompt<int>("Pick a investment [green]amount[/]?"));
                var periodSizes = ValrFeeds.All.First().PeriodSizes;
                var selectedPeriodSize = AnsiConsole.Prompt(new TextPrompt<string>("Pick a [green]PeriodSize[/]?")
                    .InvalidChoiceMessage("[red]That's not a valid strategy[/]")
                    .DefaultValue(PeriodSize.FiveMinutes.ToString())
                    .AddChoices(periodSizes.Select(x => x.ToString())));

                var periodSize = Enum.Parse<PeriodSize>(selectedPeriodSize);
                await strategyStore.Add(StrategyInstance.From(strategy, pair, amount, periodSize));
                return 0;
            }

            #endregion
        }

       
        public class List : AsyncCommandWithToken<BaseCommandSettings>
        {
            
            #region Overrides of AsyncCommandWithToken<BaseCommandSettings>

            public override async Task ExecuteAsync(BaseCommandSettings settings, CancellationToken token)
            {
                var strategyInstanceStore = IocApi.Instance.Resolve<IStrategyInstanceStore>();
                var strategyInstances = await strategyInstanceStore.Find(x => x.IsBackTest == false);
                if (!strategyInstances.Any())
                {
                    AnsiConsole.MarkupLine("No strategies yet, run [grey]`sttb stategy add`[/] to add one.");
                    return;
                }
                var table = strategyInstances.Select(x => new { Name = x.Reference, x.IsActive, x.InvestmentAmount, QuoteAmount = x.BaseAmount, x.TotalProfit });
                Console.Out.WriteLine(table.ToTable());
            }

            #endregion
        }

        public class All : AsyncCommandWithToken<BaseCommandSettings>
        {
  
            #region Overrides of AsyncCommandWithToken<BaseCommandSettings>

            public override async Task ExecuteAsync(BaseCommandSettings settings, CancellationToken token)
            {
                var strategyStore = IocApi.Instance.Resolve<IStrategyInstanceStore>();
                var strategyNames = IocApi.Instance.Resolve<StrategyPicker>().List;
                var find = await strategyStore.Find(x => true);
                const int amount = 1000;
                var count = 0;
                
                foreach (var strategy in strategyNames.Where(x=>!x.Contains("Test")))
                {
                    foreach (var (periodSize, feed) in ValrFeeds.AllWithPeriods().Where(x=>x.Item1 != PeriodSize.Week))
                    {
                        var pair = feed.CurrencyPair;
                        var exists = find.Any(x =>
                            x.StrategyName == strategy && x.Pair == pair && x.PeriodSize == periodSize &&
                            x.IsBackTest == false);
                        if (exists) continue;

                        var strategyInstance = StrategyInstance.From(strategy, pair, amount, periodSize);
                        AnsiConsole.MarkupLine($"[grey]Adding[/] [white]{strategyInstance.Name}[/].");
                        await strategyStore.Add(strategyInstance);
                        count++;
                    }
                    
                }

                AnsiConsole.MarkupLine(count == 0
                    ? "Done, [grey]looks like you have all the strategies already.[/]"
                    : $"Done, adding [green]{count}[/] strategies.");
            }

            #endregion
        }
    }
}