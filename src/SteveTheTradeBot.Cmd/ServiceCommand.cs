using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            BuildWebHost($"http://*:{settings.Port ?? "5002"}").Run();
            return 0;
        }

        public static IWebHost BuildWebHost(string port)
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Warning);
                })
                .ConfigureServices((context, collection) =>
                    collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory()))
                .UseKestrel()
                .UseUrls(port ?? "http://*:5002")
                .ConfigureAppConfiguration(SettingsFileReaderHelper)
                .UseStartup<Startup>()
                .Build();
        }

        public static void SettingsFileReaderHelper(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFilesAndEnvironment(env.EnvironmentName);
        }
    }
}