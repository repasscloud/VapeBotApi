using Telegram.Bot.Types.Payments;
using VapeBotApi.Models;
using VapeBotApi.Models.Dto;

namespace VapeBotApi.Services.Interfaces
{
    public interface IOrderService
    {
        // create_order
        Task<string?> GetCurrentNewOrderFromChatIdAsync(long chatId);
        Task<string> GenerateNewOrderFromChatIdAsync(long chatId);
        Task<List<CategoryDto>?> GetListCategoryDtoListAsync();
        Task<List<ProductDto>?> GetProductDtoListFromCategoryIdAsync(int categoryId);
        Task<bool> AddItemToCurrentNewOrderAsync(long chatId, string productId, int quantity);
        Task<bool> RemoveItemFromCurrentNewOrderAsync(long chatId, string productId, int qty);
        Task<bool> EmptyCurrentNewOrderAsync(long chatId);

        // show_cart
        Task<List<OrderItem>?> ShowCurrentNewOrderItemsAsync(long chatId);

        // checkout
        Task<decimal?> RequestCheckoutAsync(long chatId);
        Task<List<ShippingOptionDto>?> GetShippingOptionsAsync(long chatId);
        Task<bool> SetShippingCarrierAsync(long chatId, string shippingCarrier);
        Task<string?> SetPaymentMethodAsync(long chatId, string paymentMethod);

        // webapp
        Task<Order?> GetWebAppOrderAsync(string orderId);
        Task<string> FinalizeWebAppOrderAsync(Order order);

        Task<List<Order>?> GetOrdersTrackingAsync(long chatId);
        Task<List<Order>?> GetOrdersHistoryAsync(long chatId);

        Task<bool> CancelOrderAsync(string orderId);

        Task<Order?> GetOrderByPayPalInvoiceIdAsync(string invoiceId);

        Task UpdateOrderAsync(Order order);
        // bot
        // Task<string> CreateOrderGetIdAsync(long chatId);
        // Task<Order> CreateOrderAsync(long chatId);
        // Task AddItemAsync(string orderId, string productId, int quantity);
        // Task<Order> GetOrderAsync(string orderId);
        // Task<List<Order>> GetUserOrdersAsync(long chatId);
        // Task FinalizeOrderAsync(
        //     string orderId, string firstName, string secondName, string addressLine1,
        //     AUState auState, string zipCode, string mobileNo, OrderPaymentMethod method,
        //     string? addressLine2, string? addressLine3);
        // Task CancelOrderAsync(string orderId);

        // // admin
        // Task<List<Order>> GetAllOrdersAsync();
        // Task UpdateOrderPaymentReceivedAsync(string orderId);
        // Task<List<Order>?> GetPaymentReceivedOrdersAsync();


        // Task<bool> AddToCartAsync(long chatId, string productId, int qty);
        // Task<bool> SubtractFromCartAsync(long chatId, string productId, int qty);
        // Task<bool> EmptyCartAsync(long chatId);





        // // Task<string?> GetAccountLinkAsync(long chatId);
        // Task<bool> UpdateShippingDetailsAsync(Order order);
    }
}