using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SteveTheTradeBot.Core.Utils.Security
{
    public interface IKeyGen
    {
        (byte[] key, byte[] iv, byte[] salt) Generate(string data);
        (string key, string iv) Generate(string data, byte[] salt);
    }

    public class EvpBytesToKey : IKeyGen
    {
        private static  readonly Random _rnd = new Random();

        public (byte[] key, byte[] iv, byte[] salt) Generate(string data)
        {
            var salt = GenerateRandomBytes(8);
            Generate(data.Utf8ToBytes(), salt, out var key, out var iv);
            return (key, iv , salt);
        }

        public  (string key, string iv) Generate(string data, string salt)
        {
            return Generate(data, salt.HexToBytes());
        }

        public  (string key, string iv) Generate(string data, byte[] salt)
        {
            Generate(data.Utf8ToBytes(), salt, out var key, out var iv);
            return (key.BytesToHex(), iv.BytesToHex());
        }


        public  void Generate(byte[] data, byte[] salt, out byte[] key, out byte[] iv)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (salt == null || salt.Length == 0) throw new ArgumentNullException(nameof(salt));
            var hashList = new List<byte>();
            using (var hash = MD5.Create())
            {
                var dataAndSalt = Concat(data, salt);
                var currentHash = hash.ComputeHash(dataAndSalt);
                hashList.AddRange(currentHash);
                while (hashList.Count < 48) 
                {
                    var preHash = Concat(currentHash, dataAndSalt);
                    currentHash = hash.ComputeHash(preHash);
                    hashList.AddRange(currentHash);
                }
            }

            key = new byte[32];
            iv = new byte[16];
            hashList.CopyTo(0, key, 0, 32);
            hashList.CopyTo(32, iv, 0, 16);
        }

        public  byte[] GenerateRandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            _rnd.NextBytes(bytes);
            return bytes;

        }

        #region Private Methods

        private  byte[] Concat(byte[] data, byte[] salt)
        {
            var contact = new byte[data.Length + salt.Length];
            Buffer.BlockCopy(data, 0, contact, 0, data.Length);
            Buffer.BlockCopy(salt, 0, contact, data.Length, salt.Length);
            return contact;
        }

        #endregion
    }
}