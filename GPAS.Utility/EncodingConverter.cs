using System;
using System.Security.Cryptography;
using System.Text;

namespace GPAS.Utility
{
    public class EncodingConverter
    {
        public static string GetBase64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string GetMd5HashCode(string str)
        {
            StringBuilder md5Hash = new StringBuilder();
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5Provider.ComputeHash(new UTF8Encoding().GetBytes(str));
            for (int i = 0; i < bytes.Length; i++)
            {
                md5Hash.Append(bytes[i].ToString("x2"));
            }
            return md5Hash.ToString();
        }
    }
}
