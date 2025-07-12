using Microsoft.AspNetCore.Mvc;
using VapeBotApi.Models;
using VapeBotApi.Data;
using Microsoft.EntityFrameworkCore;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPriceCalculatorService _calc;
        public ProductController(AppDbContext db, IPriceCalculatorService calc)
        {
            _db = db;
            _calc = calc;
        }

        // GET: api/admin/products
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Products
                         .Include(p => p.Category)   // eager‐load the Category navigation
                         .ToListAsync());

        // GET: api/admin/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var product = await _db.Products
                                   .Include(p => p.Category)
                                   .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: api/admin/products
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
                return BadRequest("Category not found.");

            var product = new Product
            {
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                Price = _calc.CalculatePrice(dto.Price),
                CategoryId = dto.CategoryId,
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return CreatedAtAction(
                nameof(Get),
                new { id = product.ProductId },
                product
            );
        }

        // PUT: api/admin/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Product update)
        {
            if (id != update.ProductId) return BadRequest();
            _db.Entry(update).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/admin/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public class ProductCreateDto
    {
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
