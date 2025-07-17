using System.Text;
using Microsoft.EntityFrameworkCore;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Models.Dto;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly IPriceCalculatorService _calc;

        public AdminService(AppDbContext db, IPriceCalculatorService calc)
        {
            _db = db;
            _calc = calc;
        }

        #region Categories
        public Task<List<Category>> GetAllCategoriesAsync() =>
            _db.Categories.ToListAsync();

        public Task<List<Category>> GetAllCategoriesWithProductsAsync() =>
            _db.Categories
               .Include(c => c.Products)
               .ToListAsync();

        public Task<List<CategoryDto>> GetCategoriesOnlyAsync() =>
            _db.Categories
               .Select(c => new CategoryDto
               {
                   CategoryId = c.CategoryId,
                   Name = c.Name
               })
               .ToListAsync();

        public Task<CategoryNameOnlyDto?> GetCategoryNameOnlyAsync(int id) =>
            _db.Categories
               .Where(c => c.CategoryId == id)
               .Select(c => new CategoryNameOnlyDto { Name = c.Name })
               .FirstOrDefaultAsync();

        public Task<List<ProductDto>> GetProductsByCategoryIdAsync(int id) =>
            _db.Products
               .Where(p => p.CategoryId == id)
               .Select(p => new ProductDto
               {
                   ProductId = p.ProductId,
                   Name = p.Name,
                   ImageUrl = p.ImageUrl,
                   Price = p.Price
               })
               .ToListAsync();

        public async Task<Category?> GetCategoryByIdAsync(int id) =>
            await _db.Categories.FindAsync(id);

        public async Task<Category> CreateCategoryAsync(Category cat)
        {
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return cat;
        }

        public async Task<bool> UpdateCategoryAsync(Category cat)
        {
            bool exists = await _db.Categories
                                   .AnyAsync(c => c.CategoryId == cat.CategoryId);
            if (!exists) return false;

            _db.Entry(cat).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return false;

            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Products
        public async Task<List<Product>> GetAllProductsAsync() =>
            await _db.Products
                .Include(p => p.Category)   // eager‐load the Category navigation
                .ToListAsync();

        public async Task<Product?> GetProductByIdAsync(string id) =>
            await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

        public async Task<Product?> CreateProductAsync(ProductCreateDto dto)
        {
            // 1) Validate the Category exists
            var exists = await _db.Categories
                                  .AnyAsync(c => c.CategoryId == dto.CategoryId);
            if (!exists)
                return null;

            // 2) Create & save
            var product = new Product
            {
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                Price = _calc.CalculatePrice(dto.Price),
                CategoryId = dto.CategoryId,
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return product;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            // 1) Check existence
            var exists = await _db.Products
                                  .AnyAsync(p => p.ProductId == product.ProductId);
            if (!exists)
                return false;

            // 2) Mark as modified and save
            _db.Entry(product).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(string productId)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return false;

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return true;
        }
        #endregion

        #region FAQ
        public async Task<FaqItem> CreateFaqItemAsync(FaqItem item)
        {
            _db.FaqItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateFaqItemAsync(FaqItem item)
        {
            var exists = await _db.FaqItems.AnyAsync(f => f.FaqItemId == item.FaqItemId);
            if (!exists) return false;

            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFaqItemAsync(int faqItemId)
        {
            var item = await _db.FaqItems.FindAsync(faqItemId);
            if (item == null) return false;

            _db.FaqItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetFaqTextAsync()
        {
            var items = await _db.FaqItems
                                 .OrderBy(f => f.FaqItemId)
                                 .ToListAsync();

            var sb = new StringBuilder();
            foreach (var faq in items)
            {
                sb
                  .Append("<b>Q: ")
                  .Append(faq.Question)
                  .Append("</b>\n")
                  .Append("A: ")
                  .Append(faq.Answer)
                  .Append("\n\n");
            }

            return sb.ToString().Trim();
        }
        #endregion

        #region Other
        public async Task<ShippingQuote> CreateShippingQuoteAsync(ShippingQuote quote)
        {
            _db.ShippingQuotes.Add(quote);
            await _db.SaveChangesAsync();
            return quote;
        }

        public async Task<bool> UpdateShippingQuoteAsync(ShippingQuote quote)
        {
            bool exists = await _db.ShippingQuotes
                                   .AnyAsync(q => q.Id == quote.Id);
            if (!exists) return false;

            _db.Entry(quote).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteShippingQuoteAsync(int quoteId)
        {
            var q = await _db.ShippingQuotes.FindAsync(quoteId);
            if (q == null)
                return false;

            _db.ShippingQuotes.Remove(q);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ShippingQuote> GetShippingQuoteByIdAsync(int quoteId) =>
            await _db.ShippingQuotes
                .FirstOrDefaultAsync(q => q.Id == quoteId) ?? new ShippingQuote { ServiceName = "NOT EXIST" };
        #endregion
    }   
}
