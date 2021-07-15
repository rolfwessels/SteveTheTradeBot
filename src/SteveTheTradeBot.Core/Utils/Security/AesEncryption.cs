using System;
using System.Security.Cryptography;
using System.Text;

namespace SteveTheTradeBot.Core.Utils.Security
{
    /// <summary>
    /// Following the cryptojs pattern
    /// </summary>
    public class AesEncryption
    {
        private readonly RijndaelManaged _rijndaelManaged = new RijndaelManaged();

        public AesEncryption(string key, string iv) : this(key.HexToBytes(), iv.HexToBytes())
        {
            
        }

        public AesEncryption(byte[] key, byte[] iv)
        {
            _rijndaelManaged.BlockSize = 128;
            _rijndaelManaged.KeySize = 256;
            _rijndaelManaged.Padding = PaddingMode.PKCS7;
            _rijndaelManaged.Mode = CipherMode.CBC;
            _rijndaelManaged.Key = key;
            _rijndaelManaged.IV = iv;
        }

        public string Encrypt(string strPlainText)
        {
            return Convert.ToBase64String(Encrypt(strPlainText.Utf8ToBytes()));
        }

        public byte[] Encrypt(byte[] strText)
        {
            ICryptoTransform transform = _rijndaelManaged.CreateEncryptor();
            byte[] cipherText = transform.TransformFinalBlock(strText, 0, strText.Length);
            return cipherText;
        }

        public string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            return DecryptBytes(encryptedBytes);
        }

        public virtual string DecryptBytes(byte[] encryptedBytes)
        {
            var decryptor = _rijndaelManaged.CreateDecryptor(_rijndaelManaged.Key, _rijndaelManaged.IV);
            byte[] originalBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(originalBytes);
        }
    }
}