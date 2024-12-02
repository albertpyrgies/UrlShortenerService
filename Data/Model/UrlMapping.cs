namespace UrlShortenerService.Data.Model
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string ShortUrl { get; set; }
        public string LongUrl { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CustomId { get; set; }
    }
}