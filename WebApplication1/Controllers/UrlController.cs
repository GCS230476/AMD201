using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using WebApplication1.Data;
using WebApplication1.Models;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _db;

    public UrlController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("shorten")]
    public IActionResult Shorten([FromBody] ShortenRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var originalUrl = req.OriginalUrl.Trim();
        var existing = _db.UrlMappings.FirstOrDefault(u => u.OriginalUrl == originalUrl);
        if (existing != null)
        {
            var shortUrlExisting = $"{Request.Scheme}://{Request.Host}/r/{existing.ShortCode}";
            return Ok(new { shortUrl = shortUrlExisting, reused = true });
        }

        string code;
        do
        {
            code = GetRandomCode(5);
        } while (_db.UrlMappings.Any(u => u.ShortCode == code));

        var mapping = new UrlMapping
        {
            OriginalUrl = originalUrl,
            ShortCode = code,
            CreatedAt = DateTime.UtcNow
        };

        _db.UrlMappings.Add(mapping);
        _db.SaveChanges();

        var shortUrl = $"{Request.Scheme}://{Request.Host}/r/{code}";
        return Ok(new { shortUrl, reused = false });
    }

    [HttpGet("/r/{code}")]
    public async Task<IActionResult> RedirectToOriginal(string code)
    {
        var item = await _db.UrlMappings.FirstOrDefaultAsync(x => x.ShortCode == code);
        if (item == null)
            return NotFound("Short URL not found.");

        return Redirect(item.OriginalUrl);
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
}
