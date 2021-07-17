using System;
using System.Linq;

namespace SteveTheTradeBot.Core.Utils.Security
{
    public class CryptoJs 
    {
        private readonly string _sharedSecret;
        private readonly byte[] _prefix;
        private readonly IKeyGen _evpBytesToKey = new EvpBytesToKey();

        public CryptoJs(string sharedSecret, IKeyGen evpBytesToKey = null)
        {
            _sharedSecret = sharedSecret;
            _prefix = "Salted__".Utf8ToBytes();
            _evpBytesToKey = evpBytesToKey ?? new EvpBytesToKey();
        }

        #region Overrides of AesEncryption

        public string Encrypt(string plainText)
        {
            var (key, iv, salt) = _evpBytesToKey.Generate(_sharedSecret);
            var encrypted = new AesEncryption(key, iv).Encrypt(plainText.Utf8ToBytes());
            var returnEncryptedValue = ConcatPrefixAndSalt(_prefix, salt, encrypted);
            return Convert.ToBase64String(returnEncryptedValue);
        }

        protected byte[] ConcatPrefixAndSalt(byte[] prefix, byte[] salt, byte[] cipherText)
        {
            byte[] rv = new byte[prefix.Length + salt.Length + cipherText.Length];
            Buffer.BlockCopy(prefix, 0, rv, 0, prefix.Length);
            Buffer.BlockCopy(salt, 0, rv, prefix.Length, salt.Length);
            Buffer.BlockCopy(cipherText, 0, rv, prefix.Length + salt.Length, cipherText.Length);
            return rv;
        }

        public string Decrypt(string encrypted)
        {
            var  encryptedText = Convert.FromBase64String(encrypted);
            var bytes = encryptedText.Take(8).ToArray();
            if (bytes.SequenceEqual(_prefix))
            {
                var salt = encryptedText.Skip(8).Take(8).ToArray();	
                var (key, iv) = _evpBytesToKey.Generate(_sharedSecret, salt);
                return new AesEncryption(key,iv).DecryptBytes(encryptedText.Skip(16).ToArray());
            }
            throw new ArgumentException("Invalid cryptojs encryption string.");
        }

        #endregion


       
    }
}