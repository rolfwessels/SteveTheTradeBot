using System;
using System.Linq;
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
        private const string EncryptionKeyName = "EncryptionKey";
        private readonly string _configGroup;
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public BaseEncryptedSettings(IConfiguration configuration, string configGroup) : base(configuration, configGroup)
        {
            _configGroup = configGroup;
        }

        public string EncryptString(string plainText)
        {
            var encryptString = "ENC:" + Encrypt(SharedSecret(), plainText);
            return encryptString;
        }

        private string SharedSecret()
        {
            var readConfigValue = ReadConfigValue(EncryptionKeyName, EncryptionKeyName);
            if (readConfigValue == EncryptionKeyName) throw new Exception($"Please set the environment variable for `{new[] { _configGroup, EncryptionKeyName }.Where(x=> !string.IsNullOrEmpty(x)).StringJoin("__")}`.");
            return readConfigValue;
        }

        public string ReadEncryptedValue(string configValue, string defaultValue, string key = EncryptionKeyName)
        {
            var value = ReadConfigValue(configValue, defaultValue);
            if (value.StartsWith("ENC:"))
            {
                try
                {
                    var readEncryptedValue = DecryptString(SharedSecret(), value.Substring(4));
                    
                    return readEncryptedValue;
                }
                catch (Exception)
                {
                    Console.Out.WriteLine($"Fail to read {configValue} with key `{SharedSecret().Mask(2)}");
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