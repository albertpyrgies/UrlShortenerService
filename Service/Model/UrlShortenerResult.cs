using UrlShortenerService.Data.Model;

namespace UrlShortenerService.Service.Model
{
    public class UrlShortenerResult
    {
        public UrlMapping UrlMapping { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Success => string.IsNullOrEmpty(ErrorMessage);
    }
}
