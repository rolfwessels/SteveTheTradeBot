using System;
using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;
using SteveTheTradeBot.Core.Framework.Settings;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrSettings : BaseEncryptedSettings
    {
        private static Lazy<ValrSettings> _instance = new Lazy<ValrSettings>(() => new ValrSettings(new ConfigurationBuilder()
            .AddJsonFilesAndEnvironment().Build()));

        public ValrSettings(IConfiguration configuration) : base(configuration, "Valr")
        {
        }

        #region singleton

        public static ValrSettings Instance => _instance.Value;

        #endregion
        public string ApiKey => ReadEncryptedValue("ApiKey", "b9fb68df5485...");
        public string Secret => ReadEncryptedValue("Secret","b9fb68df5485...");
    }
}