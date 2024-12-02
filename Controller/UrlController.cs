using Microsoft.AspNetCore.Mvc;
using UrlShortenerService.Data.Model;
using UrlShortenerService.Service;
using UrlShortenerService.Repository;
using UrlShortenerService.Data.DTO;
using Microsoft.Extensions.Configuration;

namespace UrlShortenerService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private IUrlShortenerService _service;

        public UrlController(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _service = serviceProvider.GetRequiredService<IUrlShortenerService>();
        }

        [HttpPost]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlMappingRequest urlMapping)
        {
            if (string.IsNullOrWhiteSpace(urlMapping.LongUrl))
                return BadRequest("Long URL is required.");
            var result = await _service.ShortenUrlAsync(urlMapping.LongUrl, urlMapping.CustomId, urlMapping.Ttl);

            if (!result.Success)
                return Conflict(result.ErrorMessage); // Return conflict status if custom ID is in use

            return CreatedAtAction(nameof(RedirectToLongUrl), new { shortUrl = result.UrlMapping.ShortUrl }, result.UrlMapping);
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToLongUrl(string shortUrl)
        {
            var result = await _service.GetLongUrlAsync(shortUrl);

            if (string.IsNullOrEmpty(shortUrl) || !result.Success)
                return NotFound(result.ErrorMessage);

            return Redirect(result.UrlMapping.LongUrl);
        }

        [HttpDelete("{shortUrl}")]
        public async Task<IActionResult> DeleteShortURL(string shortURL)
        {
            var result = await _service.DeleteShortUrlAsync(shortURL);
            if (result.Success)
                return NoContent();

            return NotFound(result.ErrorMessage);
        }
    }
}
