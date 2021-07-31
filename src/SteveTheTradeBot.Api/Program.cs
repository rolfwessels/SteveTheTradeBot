using System;
using System.Linq;
using Autofac.Extensions.DependencyInjection;
using SteveTheTradeBot.Core.Framework.Logging;
using SteveTheTradeBot.Core.Framework.Settings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                BuildWebHost(args.FirstOrDefault() ?? "http://*:5002").Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHost BuildWebHost(string address)
        {
            var host = Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureServices((_, collection) =>
                            collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory()))
                        .UseKestrel()
                        .UseUrls(address)
                        .ConfigureAppConfiguration(SettingsFileReaderHelper)
                        .UseStartup<Startup>();
                }).Build();
            return host;
        }

        public static void SettingsFileReaderHelper(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            config.AddJsonFilesAndEnvironment();
        }
    }
}