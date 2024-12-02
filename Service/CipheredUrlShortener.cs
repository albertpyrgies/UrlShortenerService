using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UrlShortenerService.Data.Model;
using UrlShortenerService.Data;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Repository;
using UrlShortenerService.Service.Model;

namespace UrlShortenerService.Service
{

    public class CipheredUrlShortenerService : IUrlShortenerService
    {
        
        private readonly Dictionary<string, UrlMapping> _urlStore = new Dictionary<string, UrlMapping>(); //option to use inMemory store, DEMO ONLY
        private bool useInMemory = false; // DEMO, TODO: add a configuration option

        private readonly UrlCipher _urlCipher;

        public CipheredUrlShortenerService(string encryptionKey)
        {
            _urlCipher = new UrlCipher(encryptionKey);
        }
        public async Task<UrlShortenerResult> ShortenUrlAsync(string longUrl, string customId = null, TimeSpan? ttl = null)
        {
            if(!string.IsNullOrEmpty(customId) && _urlStore.ContainsKey(customId))
            {
                return new UrlShortenerResult { ErrorMessage = "Custom ID already in use." };
            }

            var expirationDate = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
            var urlMapping = new UrlMapping 
            { 
                LongUrl = longUrl,
                ExpirationDate = expirationDate
            };

            if (!useInMemory)
            {
                urlMapping.ShortUrl = _urlCipher.Encrypt(longUrl, expirationDate ?? DateTime.MaxValue); // Encrypt to create a short URL
                return new UrlShortenerResult { UrlMapping = urlMapping }; ;
            }
            // Generate a unique short URL
            urlMapping.ShortUrl = customId ?? GenerateShortUrl(longUrl); // Use custom ID or generate one

            // Store in memory instead of encrypted url
            if (!string.IsNullOrEmpty(customId))
            {
                _urlStore[customId] = urlMapping;
            }

            return new UrlShortenerResult { UrlMapping = urlMapping }; ;
        }

        public async Task<UrlShortenerResult> GetLongUrlAsync(string shortUrl)
        {
            UrlMapping mapping = new UrlMapping();

            if (useInMemory && !_urlStore.TryGetValue(shortUrl, out mapping))
            {
                return new UrlShortenerResult { ErrorMessage = "Short URL not found." };
            }

            if (!useInMemory)
            { 
                var decipheredValues = _urlCipher.Decrypt(shortUrl); // Decrypt to get the long URL
                mapping.ShortUrl = shortUrl;
                mapping.ExpirationDate = decipheredValues.ExpirationDate;
                mapping.LongUrl = decipheredValues.LongUrl;
            }

            if (mapping.ExpirationDate.HasValue && DateTime.UtcNow > mapping.ExpirationDate.Value)
            {
                return new UrlShortenerResult { ErrorMessage = "The short URL has expired." };
            }

            return await Task.FromResult( new UrlShortenerResult { UrlMapping = mapping });
        }

        public async Task<UrlShortenerResult> DeleteShortUrlAsync(string shortUrl)
        {
            UrlMapping mapping = new UrlMapping();
            mapping.ShortUrl = shortUrl;
            try
            {
                 _urlStore.Remove(shortUrl);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new UrlShortenerResult { ErrorMessage = "Could not remove from local storage" });
            }
            return await Task.FromResult(new UrlShortenerResult { UrlMapping = mapping });
        }
        private string GenerateUniqueId()
        {
            // Implement logic to generate a unique alphanumeric ID here.
            return Guid.NewGuid().ToString("N").Substring(0, 8); // Example of generating a unique ID
        }
        private string GenerateShortUrl(string longUrl)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(longUrl));
                return EncodeToBase62(hashBytes);
            }
        }

        private string EncodeToBase62(byte[] bytes)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder();

            BigInteger value = new BigInteger(bytes);
            while (value > 0)
            {
                result.Insert(0, chars[(int)(value % chars.Length)]);
                value /= chars.Length;
            }

            return result.ToString();
        }
    }
}
