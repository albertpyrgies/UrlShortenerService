using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UrlShortenerService.Service
{
    public class UrlCipher
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public UrlCipher(string key)
        {
            // Generate key and IV (add randomness) from the provided string
            using (var sha256 = SHA256.Create())
            {
                _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
            _iv = new byte[16]; // AES block size is 16 bytes
            Array.Copy(_key, _iv, 16); // Use part of the key as IV for simplicity
        }

        public string Encrypt(string longUrl, DateTime expirationDate)
        {
            var dataToEncrypt = $"{longUrl}|{expirationDate:o}"; // Append expiration date to URL
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(cs))
                        {
                            writer.Write(dataToEncrypt);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public (string LongUrl, DateTime ExpirationDate) Decrypt(string cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cs))
                        {
                            var decryptedData = reader.ReadToEnd();
                            var parts = decryptedData.Split('|');
                            var longUrl = parts[0];
                            var expirationDate = DateTime.Parse(parts[1]);
                            return (longUrl, expirationDate);
                        }
                    }
                }
            }
        }
    }
}
