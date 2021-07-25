using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Slack;
using Serilog.Sinks.Slack.Models;
using Spectre.Console.Cli;
using SteveTheTradeBot.Core;
using SteveTheTradeBot.Core.Framework.Logging;

namespace SteveTheTradeBot.Cmd
{
    public class Program
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<int> Main(string[] args)
        {
            Console.Title = "SteveTheTradeBot.Api";
            SetupLogin(args);
            // Console.OutputEncoding = System.Text.Encoding.UTF8;
            var app = new CommandApp();
            
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.SetApplicationName("sttb");
                config.CaseSensitivity(CaseSensitivity.None);
                config.ValidateExamples();
                config.AddCommand<ServiceCommand>("service")
                    .WithDescription("Run the web service.")
                    .WithExample(new[] { "service", "-v" });

                config.AddBranch("strategy", conf =>
                {
                    conf.SetDescription("Allows control of strategies.");
                    
                    conf.AddCommand<StrategyCommand.List>("list")
                        .WithDescription("Add a strategy.")
                        .WithExample(new[] { "strategy", "list" });
                    
                    conf.AddCommand<StrategyCommand.Add>("add")
                        .WithDescription("List strategies.")
                        .WithExample(new[] { "strategy", "add" });


                });

                config.AddBranch("data", conf =>
                {
                    conf.SetDescription("Download & import historical data.");

                    conf.AddCommand<DataCommand.Download>("download")
                        .WithAlias("import")
                        .WithDescription("Download historical data to csv files.")
                        .WithExample(new[] { "data", "download" });      
                    
                    conf.AddCommand<DataCommand.Reset>("reset")
                        .WithDescription("Reset historical data to be a few days old.")
                        .WithExample(new[] { "data", "reset","--days=5" });

                   
                });
                
            });
            try
            {
                return await app.RunAsync(args);
            }
            catch (Exception e)
            {
                _log.Error(e,e.Message);
                return 100;
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
                
                .WriteTo.File(@"c:\temp\logs\SteveTheTradeBot.Api.log", 
                    fileSizeLimitBytes: 10 * LoggingHelper.MB, 
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message} ({SourceContext}){NewLine}{Exception} ",
                    rollOnFileSizeLimit: true)
                .WriteTo.Slack(new SlackSinkOptions()
                {
                    MinimumLogEventLevel = LogEventLevel.Warning,
                    WebHookUrl = Settings.Instance.SlackWebhookUrl,
                    CustomChannel = Settings.Instance.SlackChannel,
                    BatchSizeLimit = 20,
                    Period = TimeSpan.FromSeconds(5),
                    ShowDefaultAttachments = true,
                    ShowExceptionAttachments = true,
                })
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