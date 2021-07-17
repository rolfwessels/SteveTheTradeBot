using System;
using System.Security.Cryptography;

namespace SteveTheTradeBot.Core.Utils.Security
{
    public class Pbkdf2 : IKeyGen
    {
        
        private readonly int _saltSize;
        private readonly int _iterations;
        private int _keySize;


        public Pbkdf2(int saltSize = 8 , int iterations = 10000)
        {
            
            _saltSize = saltSize;
            _iterations = iterations;
        }

        #region Implementation of IKeyGen

        public (byte[] key, byte[] iv, byte[] salt) Generate(string data)
        {
            // Generate a salt
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] salt = new byte[_saltSize];
            provider.GetBytes(salt);

            // Generate the hash
            var (key, iv) = GenerateBytes(data, salt);
           
            return  (key, iv,salt);
        }

        public (byte[] key, byte[] iv) GenerateBytes(string data, byte[] salt)
        {
            
            byte[] key;
            byte[] iv;
            using (var pbkdf2 = new Rfc2898DeriveBytes(data, salt, _iterations, HashAlgorithmName.SHA256))
            {
                _keySize = 32;
                key = pbkdf2.GetBytes(_keySize);
                iv = pbkdf2.GetBytes(16);
            }

            Console.Out.WriteLine($"salt:{salt.BytesToHex()} -  b64 {Convert.ToBase64String(salt)}");
            Console.Out.WriteLine($"key-{_iterations}:" + key.BytesToHex());
            Console.Out.WriteLine("iv:" + iv.BytesToHex());
            return (key, iv);
        }

        public (string key, string iv) Generate(string data, byte[] salt)
        {
            var (key, iv) = GenerateBytes(data, salt);
            return (key.BytesToHex(), iv.BytesToHex());
        }

        #endregion
    }
}