using System;
using Bumbershoot.Utilities;
using Bumbershoot.Utilities.Helpers;
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

        public string ApiKey => ReadEncryptedValue("ApiKey", "ENC:U2FsdGVkX1/vWYmK7v45FAj5iMotIwN9xm8\u002Bml8KH4jNvePMNctk\u002B8qMN/YSbBQeryPLEu8hAL8BuQSCM\u002BuztM3NDRW76qOflAU7CaMagDwrJDAKFq1iEkp9Ok9lWWba");
        public string Secret => ReadEncryptedValue("Secret", "ENC:U2FsdGVkX1/z69uPVQD8\u002BTxAIZx5bof9wzJ0atZq4buPJm35qXdj/3X6\u002BdEpupDZGvU44/zUCx/x\u002BbWTmVP9DmVPd77qShvWU\u002Bbxid0PCl3CPfud2YJ8WDjjyogn1zIu");
        public string ApiName => ReadConfigValue("ApiName", "ValrBrokerPaperTradingApi");
    }
}