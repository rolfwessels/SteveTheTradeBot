using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Serilog;
using Skender.Stock.Indicators;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Cmd
{
    public class StrategyCommand
    {
        public class Add : Command<Add.Settings>
        {
            
            public sealed class Settings : BaseCommandSettings
            {
                
            }

            #region Overrides of Command<Settings>

            public override int Execute(CommandContext context, Settings settings)
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
                strategyStore.Add(StrategyInstance.From(strategy, pair, amount, PeriodSize.FiveMinutes)).Wait();
                return 0;

            }

            #endregion
        }

       
        public class List : Command<BaseCommandSettings>
        {
            private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
            private readonly TimeSpan _retryIn = TimeSpan.FromMinutes(1);

            

            #region Overrides of Command<Settings>

            public override int Execute(CommandContext context, BaseCommandSettings settings)
            {
                Console.Out.WriteLine(IocApi.Instance.Resolve<StrategyPicker>().List.Select(x=>new {Name = x}).ToTable());
                return 0;
            }

            #endregion
        }
    }
}