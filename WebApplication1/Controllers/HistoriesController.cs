using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApplication1.Data;

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
        public async Task<IActionResult> Get([FromQuery] int take = 20)
        {
            var items = await _db.UrlMappings
                .OrderByDescending(x => x.Id)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.OriginalUrl,
                    x.ShortCode,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // DELETE: /api/Histories/{id}
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
