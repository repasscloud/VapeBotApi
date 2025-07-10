using Microsoft.EntityFrameworkCore;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Repositories.Interfaces;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly IUserRepository _users;
        public OrderService(AppDbContext db, IUserRepository users)
        {
            _db = db;
            _users = users;
        }

        public async Task<Order> CreateOrderAsync(long chatId)
        {
            var user = await _users.GetOrCreateAsync(chatId);
            var order = new Order { UserChatId = chatId, Status = OrderStatus.New, CreatedAt = DateTime.UtcNow };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task AddItemAsync(string orderId, int productId, int quantity)
        {
            var item = new OrderItem { OrderId = orderId, ProductId = productId, Quantity = quantity };
            _db.OrderItems.Add(item);
            await _db.SaveChangesAsync();
        }

        public async Task<Order> GetOrderAsync(string orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found.");

            return order;
        }

        public Task<List<Order>> GetUserOrdersAsync(long chatId) => _db.Orders
            .Where(o => o.UserChatId == chatId)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .ToListAsync();

        public async Task FinalizeOrderAsync(string orderId, string address, string phone, PaymentMethod method)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) throw new InvalidOperationException();

public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public AUState? State  { get; set; }
        public string? ZipCode { get; set; }
        public string? MobileNo { get; set; }



            order.ShippingAddress = address;
            order.User.SavedPhone = phone;
            order.PaymentMethod = method;
            order.Status = OrderStatus.PaymentPending;
            await _db.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) throw new InvalidOperationException();
            order.Status = OrderStatus.Canceled;
            await _db.SaveChangesAsync();
        }
    }
}
