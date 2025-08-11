using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Security.Cryptography;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public UrlController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // POST: /api/Url/shorten
        // Strong-typed binding + DataAnnotations
        [HttpPost("shorten")]
        [EnableRateLimiting("shorten-policy")]                // 🔹 rate limit
        public IActionResult Shorten([FromBody] ShortenRequest req)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var originalUrl = req.OriginalUrl.Trim();

            // 1) If you enforce 1:1 OriginalUrl mapping, reuse existing row
            var existing = _context.UrlMappings.FirstOrDefault(u => u.OriginalUrl == originalUrl);
            if (existing != null)
            {
                var existingShort = $"{Request.Scheme}://{Request.Host}/api/Url/{existing.ShortCode}";
                return Ok(new { shortUrl = existingShort, reused = true });
            }

            // 2) Use custom code if provided; ensure it's free
            string code;
            if (!string.IsNullOrWhiteSpace(req.CustomCode))
            {
                code = req.CustomCode!;
                if (_context.UrlMappings.Any(u => u.ShortCode == code))
                    return Conflict("Custom code is already taken.");
            }
            else
            {
                // 3) Generate a nice random Base62 code, small collision guard
                do { code = NewCode(7); }
                while (_context.UrlMappings.Any(u => u.ShortCode == code));
            }

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

        // GET: /api/Url/{code}  -> redirect (with cache)
        [HttpGet("{code}")]
        public IActionResult RedirectToOriginal(string code)
        {
            // 🔹 check cache first
            if (!_cache.TryGetValue(code, out string? original))
            {
                original = _context.UrlMappings
                    .Where(u => u.ShortCode == code)
                    .Select(u => u.OriginalUrl)
                    .FirstOrDefault();

                if (original == null)
                    return NotFound("Short URL not found.");

                // cache for 10 minutes
                _cache.Set(code, original, TimeSpan.FromMinutes(10));
            }

            return Redirect(original);
        }

        private static string NewCode(int len = 7)
        {
            const string set = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = RandomNumberGenerator.GetBytes(len);
            var chars = new char[len];
            for (int i = 0; i < len; i++) chars[i] = set[bytes[i] % set.Length];
            return new string(chars);
        }
    }
}
