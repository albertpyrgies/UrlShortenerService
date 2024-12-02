namespace UrlShortenerService.Data.DTO
{
    public class UrlMappingRequest
    {
        public string LongUrl { get; set; }
        public string? CustomId { get; set; } // Optional custom ID
        public TimeSpan? Ttl { get; set; } // Optional TTL for expiration
    }
}
