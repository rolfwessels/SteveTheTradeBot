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
                config.CaseSensitivity(CaseSensitivity.None);
                config.ValidateExamples();
                config.AddCommand<ServiceCommand>("service")
                    .WithDescription("Run the web service.")
                    .WithExample(new[] { "service", "-v" });
                config.AddCommand<DataImportCommand>("import")
                    .WithDescription("Build candle stick data from historical trades.")
                    .WithExample(new[] { "import", "-v" });

                config.AddBranch("data", student =>
                {
                    student.SetDescription("Download & import historical data.");

                    student.AddCommand<DataCommand.Download>("download")
                        .WithAlias("import")
                        .WithDescription("Download historical data to csv files.")
                        .WithExample(new[] { "data", "download" });

                   
                });
                
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
                .WriteTo.Console(RestrictedToMinimumLevel(args))
                //.ReadFrom.Configuration(BaseSettings.Config)
                .CreateLogger());

        }

        private static LogEventLevel RestrictedToMinimumLevel(string[] args)
        {
            if (args.Any(x => x == "--vv")) return LogEventLevel.Debug;
            return args.Any(x => x == "-v") ? LogEventLevel.Information : LogEventLevel.Warning;
        }
    }

    
}