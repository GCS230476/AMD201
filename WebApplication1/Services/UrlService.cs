using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class UrlService
    {
        private readonly AppDbContext _context;

        public UrlService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> ShortenUrl(string originalUrl)
        {
            // Generate a simple short code (6 characters from GUID)
            string shortCode = Guid.NewGuid().ToString("N").Substring(0, 6);

            var url = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode
            };

            _context.UrlMappings.Add(url);
            await _context.SaveChangesAsync();

            return shortCode;
        }

        public async Task<string?> GetOriginalUrl(string shortCode)
        {
            var record = await _context.UrlMappings
                .FindAsync(shortCode);

            if (record == null)
            {
                // Try searching manually if FindAsync fails (GUID vs key issue)
                record = _context.UrlMappings
                    .FirstOrDefault(u => u.ShortCode == shortCode);
            }

            return record?.OriginalUrl;
        }
    }
}
