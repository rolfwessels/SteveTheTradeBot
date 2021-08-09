using System;
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
        public string RedisHost => ReadConfigValue("RedisHost", "localhost:6391");
        public string SlackBotKey => ReadEncryptedValue("SlackBotKey", "ENC:U2FsdGVkX1/aMryp50WI/N7Sx3Mo6zIBHbdqAn4Rv8KGJGi897/ETE2bYjQglxxperpnQvp2e9Dr9NlP931+dLbs6U7PPWzFp+32DVVYxsA=");
        public string SlackChannel => ReadConfigValue("SlackChannel", "#steve-trader-dev");
        public string LogsSlackChannel => ReadConfigValue("LogsSlackChannel", "#steve-trader-dev-logs");
        public string SlackWebhookUrl => ReadEncryptedValue("SlackWebhookUrl", "ENC:U2FsdGVkX19BcF7bp+Ckgo2z5hu8nEu6eFOQ0hNXlkIEM42tMZ04bcOXVfUQiBjD4sPxCUmwPBzNWzaUpEPVu/T3mOmxz9T6aXDQyNL7sTNOuBEpVQksHctzpe2gq/EF");
        public string LogFolder => ReadEncryptedValue("LogFolder", @"c:\temp\logs\");
        public string DataProtectionFolder => ReadEncryptedValue("DataProtectionFolder", @"c:\temp\sttb\");
        public string LokiUrl => ReadConfigValue("LokiUrl", @"https://logs-prod-us-central1.grafana.net");
        public string LokiUser => ReadEncryptedValue("LokiUser", @"ENC:U2FsdGVkX19MEoMc1/5vkmPF+x/85lRb36RexmGYgAc=");
        public string LokiPassword => ReadEncryptedValue("LokiPassword", @"ENC:U2FsdGVkX1+byC3zaxv3cRZgmvoE0q6+0yiO0fikKBjxloRPGB9CRCOkWqd+5Z6TEMQT5oO3kvK9JB+Z/inz7qSZ209sZLHsti48AAvBeseXovrzm+UCegeHKJlqXTC50TIUNC2Y1X7E2w6Uqbqhy2LO9ca01BTOWU9gCjN2ijo=");
    }
}
