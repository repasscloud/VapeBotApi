using VapeBotApi.Models;

namespace VapeBotApi.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(long chatId);
        Task AddItemAsync(string orderId, string productId, int quantity);
        Task<Order> GetOrderAsync(string orderId);
        Task<List<Order>> GetUserOrdersAsync(long chatId);
        Task FinalizeOrderAsync(
            string orderId, string firstName, string secondName, string addressLine1,
            AUState auState, string zipCode, string mobileNo, PaymentMethod method,
            string? addressLine2, string? addressLine3);
        Task CancelOrderAsync(string orderId);
    }
}