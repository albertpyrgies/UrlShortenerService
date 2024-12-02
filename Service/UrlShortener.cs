using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UrlShortenerService.Data.Model;
using UrlShortenerService.Data;
using Microsoft.EntityFrameworkCore;

namespace UrlShortenerService.Service
{
    
    public class UrlShortener
    {
        

        public string GenerateShortUrl(string longUrl)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(longUrl));
                // Convert hash bytes to Base62
                return EncodeToBase62(hashBytes);
            }

        }

        public string EncodeToBase62(byte[] bytes)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder();

            // Convert bytes to decimal and then to Base62
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
