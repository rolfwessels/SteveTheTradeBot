using System;
using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core
{
    public class Settings : BaseEncryptedSettings
    {
        private static Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true).Build()));

        public Settings(IConfiguration configuration) : base(configuration, null)
        {
        }

        #region singletoncd 

        public static Settings Instance => _instance.Value;

        #endregion

        public string MongoConnection => ReadConfigValue("MongoConnection", "mongodb://localhost:27022/");
        public string MongoDatabase => ReadConfigValue("MongoDatabase", "SteveTheTradeBotSample");
        public string NpgsqlConnection => ReadConfigValue("NpgsqlConnection", "Host=localhost;Database=SteveTheTradeBotSample;Username=postgres;Password=GRES_password;Port=15432");
        public string WebBasePath => ReadConfigValue("WebBasePath", null);
        public string RedisHost => ReadConfigValue("RedisHost", "localhost:6391");
        public string SlackBotKey => ReadEncryptedValue("SlackBotKey", "ENC:U2FsdGVkX18yL23DTXaiC3o+A+ITplG3beoAPrnfOnVi1sN9p0hhzw66pTf7OvL/+/zKJpiRGkRLBVRADq1ODsOwVRP/BKilNikvqJMLon8=");
        public string SlackChannel => ReadConfigValue("SlackChannel", "#steve-trader-dev");
        public string SlackWebhookUrl => ReadEncryptedValue("SlackWebhookUrl", "ENC:U2FsdGVkX1/XkFGjcelJyvYGKfS+NKj8kbEF1wCF6X4IH9zvN+8o+7K1H4kTahK0t8MMmhIWeWhx6tAE8UzFiqi7hgq18PaSkJJKcCSdBIzV+1Lq2kG++FGebXEuVZKl");

        public static void Initialize(IConfiguration configuration)
        {
            _instance = new Lazy<Settings>(() => new Settings(configuration));
        }
    }
}
