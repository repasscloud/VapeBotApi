using Telegram.Bot.Types.Payments;
using VapeBotApi.Models;
using VapeBotApi.Models.Dto;

namespace VapeBotApi.Services.Interfaces
{
    public interface IOrderService
    {
        // bot
        Task<string> CreateOrderGetIdAsync(long chatId);
        Task<Order> CreateOrderAsync(long chatId);
        Task AddItemAsync(string orderId, string productId, int quantity);
        Task<Order> GetOrderAsync(string orderId);
        Task<List<Order>> GetUserOrdersAsync(long chatId);
        Task FinalizeOrderAsync(
            string orderId, string firstName, string secondName, string addressLine1,
            AUState auState, string zipCode, string mobileNo, OrderPaymentMethod method,
            string? addressLine2, string? addressLine3);
        Task CancelOrderAsync(string orderId);

        // admin
        Task<List<Order>> GetAllOrdersAsync();
        Task UpdateOrderPaymentReceivedAsync(string orderId);
        Task<List<Order>?> GetPaymentReceivedOrdersAsync();
        Task UpdateTrackingInfo(string orderId, OrderStatus status, ShippingCarrier carrier, string TrackingNumber);

        Task<bool> AddToCartAsync(long chatId, string productId, int qty);
        Task<bool> SubtractFromCartAsync(long chatId, string productId, int qty);
        Task<bool> EmptyCartAsync(long chatId);
        Task<decimal> RequestCheckoutAsync(long chatId);
        Task<decimal> SetShippingCarrierAsync(long chatId, string shippingCarrier);
        Task<int> SetPaymentMethodAsync(long chatId, string paymentMethod);

        Task<IEnumerable<ShippingOptionDto>> GetShippingOptionsAsync(long chatId);
        Task<string?> GetAccountLinkAsync(long chatId);
        Task<bool> UpdateShippingDetailsAsync(Order order);
    }
}