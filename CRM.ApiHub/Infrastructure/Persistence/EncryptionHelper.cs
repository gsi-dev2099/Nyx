using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CRM.ApiHub.Infrastructure.Persistence;

public static class EncryptionHelper
{
    private static readonly byte[] DefaultKey = Encoding.UTF8.GetBytes("NyxCRMDatabaseKeySecret2026Secur"); // 32 bytes
    private static readonly byte[] DefaultIv = Encoding.UTF8.GetBytes("NyxCRMInitVector"); // 16 bytes

    private static byte[] GetKey()
    {
        var keyStr = Environment.GetEnvironmentVariable("NYX_DB_ENCRYPTION_KEY");
        if (string.IsNullOrEmpty(keyStr))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env == "Development")
            {
                Console.WriteLine("[WARNING] NYX_DB_ENCRYPTION_KEY is not set. Using insecure development fallback key.");
                return DefaultKey;
            }
            throw new InvalidOperationException("La variable de entorno 'NYX_DB_ENCRYPTION_KEY' debe estar configurada en producción.");
        }
        return Encoding.UTF8.GetBytes(keyStr.PadRight(32).Substring(0, 32));
    }

    private static byte[] GetIv()
    {
        var ivStr = Environment.GetEnvironmentVariable("NYX_DB_ENCRYPTION_IV");
        if (string.IsNullOrEmpty(ivStr))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env == "Development")
            {
                return DefaultIv;
            }
            throw new InvalidOperationException("La variable de entorno 'NYX_DB_ENCRYPTION_IV' debe estar configurada en producción.");
        }
        return Encoding.UTF8.GetBytes(ivStr.PadRight(16).Substring(0, 16));
    }

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = GetIv();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
        }

        return "Encrypted:" + Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        if (!cipherText.StartsWith("Encrypted:")) return cipherText;

        var cleanCipherText = cipherText.Substring(10);
        var cipherBytes = Convert.FromBase64String(cleanCipherText);

        using var aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = GetIv();

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
