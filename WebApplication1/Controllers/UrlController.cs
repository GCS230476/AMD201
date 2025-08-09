using Microsoft.AspNetCore.Mvc;
using System.Linq;
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

        // POST: /api/Url/shorten
        [HttpPost("shorten")]
        public IActionResult Shorten([FromBody] string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                return BadRequest("URL cannot be empty.");

            var shortCode = Guid.NewGuid().ToString("N").Substring(0, 6);

            var mapping = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode
            };

            _context.UrlMappings.Add(mapping);
            _context.SaveChanges();

            var shortUrl = $"{Request.Scheme}://{Request.Host}/api/Url/{shortCode}";
            return Ok(new { shortUrl });
        }

        // GET: /api/Url/{code}
        [HttpGet("/api/Url/{code}")]
        public IActionResult RedirectToOriginal(string code)
        {
            var mapping = _context.UrlMappings.FirstOrDefault(u => u.ShortCode == code);

            if (mapping == null)
                return NotFound("Short URL not found.");

            return Redirect(mapping.OriginalUrl);
        }
    }
}
