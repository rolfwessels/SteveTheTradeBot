using System;
using System.Reflection;
using Bumbershoot.Utilities;
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
            var encryptString = "ENC:" + Encrypt(ReadConfigValue("EncryptionKey", "EncryptionKey"), plainText);
            return encryptString;
        }
        
        public string ReadEncryptedValue(string configValue, string defaultValue, string key = "EncryptionKey")
        {
            var value = ReadConfigValue(configValue,defaultValue);
            if (value.StartsWith("ENC:"))
            {
                try
                {
                    return DecryptString(ReadConfigValue("EncryptionKey", "EncryptionKey"), value.Substring(4));
                }
                catch (Exception)
                {
                    _log.Warning($"BaseEncryptedSettings:ReadEncryptedValue could not decrypt `{value.Substring(0,Math.Min(10, value.Length))}...`");
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