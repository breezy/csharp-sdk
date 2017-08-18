using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Breezy.Sdk
{
    internal static class Util
    {
        public static Dictionary<string, string> ParseFormContent(string contentRaw)
        {
            var result = new Dictionary<string, string>();

            foreach (var pair in contentRaw.Split('&'))
            {
                var keyValueSplitted = pair.Split('=');
                result.Add(keyValueSplitted[0], Uri.UnescapeDataString(keyValueSplitted[1]));
            }

            return result;
        }

        public static string GetHmacSha256Signature(string toSign, string encryptionKey)
        {
            var key = Encoding.ASCII.GetBytes(encryptionKey);
            var bytesToSign = Encoding.ASCII.GetBytes(toSign);
            using (var hmac = new HMACSHA256(key))
            {
                var signatureBytes = hmac.ComputeHash(bytesToSign);
                return Convert.ToBase64String(signatureBytes);
            }
        }

        public static byte[] DecodeHexString(string data)
        {
            if (data == null) throw new ArgumentNullException("data");
            data = data.Replace(" ", string.Empty);

            int length = (data.Length) / 2;
            byte[] ar = new byte[length];
            for (int i = 0; i < length; i++)
            {
                string subString = data.Substring(2 * i, 2);
                ar[i] = Convert.ToByte(subString, 16);
            }
            return ar;
        }
    }
}