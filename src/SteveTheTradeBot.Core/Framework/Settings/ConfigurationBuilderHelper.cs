using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace SteveTheTradeBot.Core.Framework.Settings
{
    public static class ConfigurationBuilderHelper
    {
        public static IConfigurationBuilder AddJsonFilesAndEnvironment(this IConfigurationBuilder config)
        {
            var environment = GetEnvironment();
            config.AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);
            config.AddEnvironmentVariables();
            return config;
        }

        public static string GetEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            return environment;
        }

        public static string InformationalVersion()
        {
            return Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
        }
    }
}