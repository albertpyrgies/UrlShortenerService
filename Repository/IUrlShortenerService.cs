using UrlShortenerService.Data.Model;
using UrlShortenerService.Service.Model;

namespace UrlShortenerService.Repository
{
    public interface IUrlShortenerService
    {
        Task<UrlShortenerResult> ShortenUrlAsync(string longUrl, string? customId = null, TimeSpan? ttl = null);
        Task<UrlShortenerResult> GetLongUrlAsync(string shortUrl);
        Task<UrlShortenerResult> DeleteShortUrlAsync(string shortUrl);
    }
}
