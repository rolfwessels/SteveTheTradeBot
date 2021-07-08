using System;
using System.Linq;
using System.Reflection;
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
using ILogger = Serilog.ILogger;

namespace SteveTheTradeBot.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "SteveTheTradeBot.Api";

            Log.Logger = LoggingHelper.SetupOnce(() => new LoggerConfiguration().MinimumLevel.Debug()
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