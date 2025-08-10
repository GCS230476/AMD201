using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UrlController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("shorten")]
        [Consumes("application/json", "text/plain")]
        public IActionResult Shorten([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                return BadRequest("URL cannot be empty.");

            originalUrl = originalUrl.Trim();

            // 1) Reuse if exists
            var existing = _context.UrlMappings.FirstOrDefault(u => u.OriginalUrl == originalUrl);
            if (existing != null)
            {
                var existingShort = $"{Request.Scheme}://{Request.Host}/api/Url/{existing.ShortCode}";
                return Ok(new { shortUrl = existingShort, reused = true });
            }

            // 2) Otherwise create a unique new code
            string code;
            do
            {
                code = GetRandomCode(6);
            } while (_context.UrlMappings.Any(u => u.ShortCode == code)); // collision guard

            var mapping = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = code,
                CreatedAt = DateTime.UtcNow
            };

            _context.UrlMappings.Add(mapping);
            _context.SaveChanges();

            var shortUrl = $"{Request.Scheme}://{Request.Host}/api/Url/{code}";
            return Ok(new { shortUrl, reused = false });
        }

        private static string GetRandomCode(int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = alphabet[bytes[i] % alphabet.Length];
            return new string(chars);
        }

        [HttpGet("{code}")]
        public IActionResult RedirectToOriginal(string code)
        {
            var mapping = _context.UrlMappings.FirstOrDefault(u => u.ShortCode == code);
            if (mapping == null) return NotFound("Short URL not found.");
            return Redirect(mapping.OriginalUrl);
        }
    }
}
