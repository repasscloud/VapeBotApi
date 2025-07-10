using VapeBotApi.Models;

namespace VapeBotApi.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(long chatId);
        Task AddItemAsync(Guid orderId, int productId, int quantity);
        Task<Order> GetOrderAsync(Guid orderId);
        Task<List<Order>> GetUserOrdersAsync(long chatId);
        Task FinalizeOrderAsync(Guid orderId, string address, string phone, PaymentMethod method);
        Task CancelOrderAsync(Guid orderId);
    }
}