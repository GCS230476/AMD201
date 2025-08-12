using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using System.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HistoriesController(AppDbContext db) => _db = db;

        // GET: /api/Histories?take=20
        [HttpGet]
        public IActionResult Get([FromQuery] int take = 20)
        {
            var items = _db.UrlMappings
                .OrderByDescending(x => x.Id)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.OriginalUrl,
                    x.ShortCode,
                    x.CreatedAt
                })
                .ToList();

            return Ok(items);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var history = await _db.UrlMappings.FindAsync(id);
            if (history == null)
                return NotFound();

            _db.UrlMappings.Remove(history);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
