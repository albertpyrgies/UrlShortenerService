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

    public class DbUrlShortenerService : IUrlShortenerService
    {
        private readonly AppDbContext _context;
        private UrlShortener _urlShortener = new UrlShortener();

        public DbUrlShortenerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UrlShortenerResult> ShortenUrlAsync(string longUrl, string customId = null, TimeSpan? ttl = null)
        {
            if (!string.IsNullOrEmpty(customId) && await _context.UrlMappings.AnyAsync(u => u.ShortUrl == customId))
            {
                return new UrlShortenerResult { ErrorMessage = "Custom ID already in use." };
            }
            var expirationDate = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
            var urlMapping = new UrlMapping 
            { 
                LongUrl = longUrl,
                ShortUrl = customId ?? _urlShortener.GenerateShortUrl(longUrl),
                ExpirationDate = expirationDate
            };
            

            while (await _context.UrlMappings.AnyAsync(u => u.ShortUrl == urlMapping.ShortUrl))
            {
                urlMapping.ShortUrl = _urlShortener.GenerateShortUrl(longUrl + Guid.NewGuid().ToString());
            }

            await _context.UrlMappings.AddAsync(urlMapping);
            await _context.SaveChangesAsync();

            return new UrlShortenerResult { UrlMapping = urlMapping };
        }

        public async Task<UrlShortenerResult> GetLongUrlAsync(string shortUrl)
        {
            var mapping = await _context.UrlMappings.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);

            if (mapping == null)
            {
                return new UrlShortenerResult { ErrorMessage = "Short URL not found." };
            }

            if (mapping.ExpirationDate.HasValue && DateTime.UtcNow > mapping.ExpirationDate.Value)
            {
                return new UrlShortenerResult { ErrorMessage = "The short URL has expired." };
            }

            return new UrlShortenerResult { UrlMapping = mapping };
        }

        public async Task<UrlShortenerResult> DeleteShortUrlAsync(string shortUrl)
        {
            
            var mapping = await _context.UrlMappings.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);
            if (mapping == null)
                return await Task.FromResult(new UrlShortenerResult { ErrorMessage = "Url does not exist" }); ;

            try
            {
                _context.UrlMappings.Remove(mapping);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new UrlShortenerResult { ErrorMessage = "Could not remove from local storage" });
            }
            return await Task.FromResult(new UrlShortenerResult { UrlMapping = mapping });
        }

    }
}