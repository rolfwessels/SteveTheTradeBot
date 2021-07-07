using Microsoft.Extensions.Configuration;

namespace SteveTheTradeBot.Core.Framework.Settings
{
    public static class ConfigurationBuilderHelper
    {
        public static IConfigurationBuilder AddJsonFilesAndEnvironment(this IConfigurationBuilder config,
            string environment = "Development")
        {
            config.AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);
            config.AddEnvironmentVariables();
            return config;
        }
    }
}