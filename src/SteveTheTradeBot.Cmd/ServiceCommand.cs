using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api;
using SteveTheTradeBot.Core.Framework.Settings;

namespace SteveTheTradeBot.Cmd
{
    public sealed class ServiceCommand : Command<ServiceCommand.Settings>
    {
        public sealed class Settings : BaseCommandSettings
        {
            [CommandOption("-p")]
            [Description("Port [5002]")]
            public string Port { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            // var cancellationTokenSource = ConsoleHelper.BindToCancelKey();
            Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureServices((_, collection) => collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory()))
                        .UseKestrel()
                        .UseUrls($"http://*:{settings.Port ?? "5002"}")
                        .ConfigureAppConfiguration(SettingsFileReaderHelper)
                        .UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<TickerTrackerService>();
                    services.AddHostedService<PopulateOneMinuteCandleService>();
                    services.AddHostedService<PopulateOtherCandlesService>();
                    services.AddHostedService<PopulateOtherMetrics>();
                    services.AddHostedService<StrategyService>();
                    services.AddHostedService<SlackAlertService>();
                })
                .Build()
                .Run();
            return 0;
        }

        public static void SettingsFileReaderHelper(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            config.AddJsonFilesAndEnvironment();
        }
    }
}