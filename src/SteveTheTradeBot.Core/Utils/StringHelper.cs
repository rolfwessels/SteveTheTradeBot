using System;
using System.Linq;
using System.Text;

namespace SteveTheTradeBot.Core.Utils
{
    public static class StringHelper
    {
        public static string ToHexString(this byte[] hash)
        {
            StringBuilder result = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }

        public static byte[] HexToBytes(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static byte[] Utf8ToBytes(this string strPlainText)
        {
            return new UTF8Encoding().GetBytes(strPlainText);
        }

        public static string BytesToHex(this byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }
    }
}