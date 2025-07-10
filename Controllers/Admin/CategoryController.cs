using Microsoft.AspNetCore.Mvc;
using VapeBotApi.Models;
using VapeBotApi.Data;
using Microsoft.EntityFrameworkCore;

namespace VapeBotApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CategoryController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Categories.Include(c => c.Products).ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return NotFound();
            return Ok(c);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category cat)
        {
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = cat.CategoryId }, cat);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Category update)
        {
            if (id != update.CategoryId) return BadRequest();
            _db.Entry(update).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return NotFound();
            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
