using System;
using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;

namespace SteveTheTradeBot.Core
{
    public class Settings : BaseSettings
    {
        private static Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true).Build()));

        public Settings(IConfiguration configuration) : base(configuration, null)
        {
        }

        #region singleton

        public static Settings Instance => _instance.Value;

        #endregion

        public string MongoConnection => ReadConfigValue("MongoConnection", "mongodb://localhost/");
        public string MongoDatabase => ReadConfigValue("MongoDatabase", "SteveTheTradeBotSample");
        public string NpgsqlConnection => ReadConfigValue("NpgsqlConnection", "Host=localhost;Database=SteveTheTradeBotSample;Username=postgres;Password=GRES_password");
        public string WebBasePath => ReadConfigValue("WebBasePath",null);
        public string RedisHost => ReadConfigValue("RedisHost", "localhost:6390");

        public static void Initialize(IConfiguration configuration)
        {
            _instance = new Lazy<Settings>(() => new Settings(configuration));
        }
    }

}
