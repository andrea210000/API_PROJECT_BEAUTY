using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;

namespace API_PROJECT_BEAUTY.Tools
{
    public class CryptoService
    {
        // 32 bytes = 256 bits
        private static readonly byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");

        // 16 bytes = 128 bits
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();

            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new(cs);

            sw.Write(plainText);
            sw.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            using Aes aes = Aes.Create();

            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] buffer = Convert.FromBase64String(cipherText);

            using MemoryStream ms = new(buffer);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);

            return sr.ReadToEnd();
        }
    }
}
