using System;
using System.Reflection;
using Bumbershoot.Utilities;
using Bumbershoot.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using Serilog;
using SteveTheTradeBot.Core.Utils.Security;

namespace SteveTheTradeBot.Core.Utils
{
    public class BaseEncryptedSettings : BaseSettings
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public BaseEncryptedSettings(IConfiguration configuration, string configGroup) : base(configuration, configGroup)
        {
        }

        public string EncryptString(string plainText)
        {
            var encryptString = "ENC:" + Encrypt(SharedSecret(), plainText);
            return encryptString;
        }

        private string SharedSecret()
        {
            return ReadConfigValue("EncryptionKey", "EncryptionKey");
        }

        public string ReadEncryptedValue(string configValue, string defaultValue, string key = "EncryptionKey")
        {
            var value = ReadConfigValue(configValue, defaultValue);
            if (value.StartsWith("ENC:"))
            {
                try
                {
                    return DecryptString(SharedSecret(), value.Substring(4));
                }
                catch (Exception)
                {
                    _log.Warning($"BaseEncryptedSettings:ReadEncryptedValue could not decrypt {configValue}: with key `{SharedSecret().Mask(2)}` `{value.Mask(10)}...`");
                    return defaultValue;
                }
            }
            return value;
        }

        private string DecryptString(string sharedSecret, string encrypted)
        {
            return new CryptoJs(sharedSecret).Decrypt(encrypted);
        }

        protected virtual string Encrypt(string sharedSecret, string plainText)
        {
            return new CryptoJs(sharedSecret).Encrypt(plainText);
        }
    }

}