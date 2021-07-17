using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Spectre.Console.Cli;
using SteveTheTradeBot.Core.Framework.Logging;

namespace SteveTheTradeBot.Cmd
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Console.Title = "SteveTheTradeBot.Api";
            SetupLogin(args);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationName("sttb");
                config.ValidateExamples();
                config.AddCommand<ServiceCommand>("service")
                    .WithDescription("Run the web service.")
                    .WithExample(new[] { "service", "-v" });
                config.AddCommand<DataImportCommand>("import")
                    .WithDescription("Build candle stick data from historical trades.")
                    .WithExample(new[] { "import", "-v" });
            });
            try
            {
                return await app.RunAsync(args);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void SetupLogin(string[] args)
        {
            Log.Logger = LoggingHelper.SetupOnce(() => new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(@"c:\temp\logs\SteveTheTradeBot.Api.log", fileSizeLimitBytes: 10 * LoggingHelper.MB,
                    rollOnFileSizeLimit: true)
                .WriteTo.Console(args.Any(x => x == "-v") ? LogEventLevel.Debug : LogEventLevel.Information)
                //.ReadFrom.Configuration(BaseSettings.Config)
                .CreateLogger());

        }
    }
}