using System;
using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;
using SteveTheTradeBot.Core.Framework.Settings;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core
{
    public class Settings : BaseEncryptedSettings
    {
        private static Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings(new ConfigurationBuilder()
            .AddJsonFilesAndEnvironment().Build()));

        public Settings(IConfiguration configuration) : base(configuration, null)
        {
        }

        #region singletoncd 

        public static Settings Instance => _instance.Value;

        #endregion

        public string MongoConnection => ReadEncryptedValue("MongoConnection", "mongodb://localhost:27022/SteveTheTradeBotSample");
        public string NpgsqlConnection => ReadEncryptedValue("NpgsqlConnection", "Host=localhost;Database=SteveTheTradeBotSample;Username=postgres;Password=GRES_password;Port=15432");
        public string WebBasePath => ReadConfigValue("WebBasePath", null);
        public string RedisHost => ReadConfigValue("RedisHost", "localhost:6391");
        public string SlackBotKey => ReadEncryptedValue("SlackBotKey", "ENC:U2FsdGVkX18yL23DTXaiC3o+A+ITplG3beoAPrnfOnVi1sN9p0hhzw66pTf7OvL/+/zKJpiRGkRLBVRADq1ODsOwVRP/BKilNikvqJMLon8=");
        public string SlackChannel => ReadConfigValue("SlackChannel", "#steve-trader-dev");
        public string LogsSlackChannel => ReadConfigValue("SlackChannel", "#steve-trader-dev-logs");
        public string SlackWebhookUrl => ReadEncryptedValue("SlackWebhookUrl", "ENC:U2FsdGVkX1/XkFGjcelJyvYGKfS+NKj8kbEF1wCF6X4IH9zvN+8o+7K1H4kTahK0t8MMmhIWeWhx6tAE8UzFiqi7hgq18PaSkJJKcCSdBIzV+1Lq2kG++FGebXEuVZKl");
        public string LogFolder => ReadEncryptedValue("LogFolder", @"c:\temp\logs\");

    }
}
