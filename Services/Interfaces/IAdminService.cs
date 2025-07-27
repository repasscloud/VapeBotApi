using VapeBotApi.Models;
using VapeBotApi.Models.Admin;
using VapeBotApi.Models.Dto;

namespace VapeBotApi.Services.Interfaces
{
    public interface IAdminService
    {
        // CATEGORIES
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Category>> GetAllCategoriesWithProductsAsync();
        Task<List<CategoryDto>> GetCategoriesOnlyAsync();
        Task<CategoryNameOnlyDto?> GetCategoryNameOnlyAsync(int id);
        Task<List<ProductDto>> GetProductsByCategoryIdAsync(int id);
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category cat);
        Task<bool> UpdateCategoryAsync(Category cat);
        Task<bool> DeleteCategoryAsync(int id);

        // PRODUCTS
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(string id);
        Task<Product?> CreateProductAsync(ProductCreateDto dto);
        Task<bool> UpdateProductAsync(Product productToUpdate);
        Task<bool> DeleteProductAsync(string productId);

        // FAQ
        Task<FaqItem> CreateFaqItemAsync(FaqItem item);
        Task<bool> UpdateFaqItemAsync(FaqItem item);
        Task<bool> DeleteFaqItemAsync(int faqItemId);
        Task<string> GetFaqTextAsync();

        // OTHER
        Task<ShippingQuote> CreateShippingQuoteAsync(ShippingQuote quote);
        Task<bool> UpdateShippingQuoteAsync(ShippingQuote quote);
        Task<bool> DeleteShippingQuoteAsync(int quoteId);
        Task<ShippingQuote> GetShippingQuoteByIdAsync(int quoteId);

        Task UpdateTrackingInfo(string orderId, OrderStatus status, string TrackingNumber);

        Task<string> DeleteAccountByChatIdAsync(long chatId);

        Task<string> CreateContactSupportMsgAsync(CustomerMessage cm);
    }
}