using System;
using System.Linq;
using SteveTheTradeBot.Core.Framework.Logging;
using SteveTheTradeBot.Core.Framework.Settings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace SteveTheTradeBot.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "SteveTheTradeBot.Api";
            Log.Logger = LoggingHelper.SetupOnce(() => new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(@"c:\temp\logs\SteveTheTradeBot.Api.log", fileSizeLimitBytes: 10 * LoggingHelper.MB,
                    rollOnFileSizeLimit: true)
                .WriteTo.Console(LogEventLevel.Information)
                //.ReadFrom.Configuration(BaseSettings.Config)
                .CreateLogger());

            try
            {
                BuildWebHost(args).Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Warning);
                })
                .ConfigureServices((context, collection) =>
                    collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory()))
                .UseKestrel()
                .UseUrls(args.FirstOrDefault() ?? "http://*:5002")
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