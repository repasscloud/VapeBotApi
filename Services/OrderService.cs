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
            var order = new Order { UserChatId = chatId, LastUpdated = DateTime.UtcNow };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task AddItemAsync(string orderId, string productId, int quantity)
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

        public async Task FinalizeOrderAsync(
            string orderId, string firstName, string secondName, string addressLine1,
            AUState auState, string zipCode, string mobileNo, PaymentMethod method,
            string? addressLine2, string? addressLine3)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) throw new InvalidOperationException();

            order.FirstName = firstName;
            order.SecondName = secondName;
            order.AddressLine1 = addressLine1;
            order.State = auState;
            order.ZipCode = zipCode;
            order.MobileNo = mobileNo;

            if (!string.IsNullOrWhiteSpace(addressLine2))
            {
                order.AddressLine2 = addressLine2;
            }

            if (!string.IsNullOrWhiteSpace(addressLine3))
            {
                order.AddressLine3 = addressLine3;
            }

            order.PaymentMethod = method;
            order.Status = OrderStatus.PaymentPending;
            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(string orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) throw new InvalidOperationException();
            order.Status = OrderStatus.Canceled;
            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // admin
        public async Task UpdateOrderPaymentReceivedAsync(string orderId)
        {
            var order = await _db.Orders
                                .FindAsync(orderId);
            if (order is not null)
            {
                order.Status = OrderStatus.PaymentReceived;
                order.LastUpdated = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // update the telegram API with the notification to the user now
            }
            else
            {
                // log error here
            }
        }
        public async Task<List<Order>?> GetPaymentReceivedOrdersAsync()
        {
            var list = await _db.Orders
                               .Where(o => o.Status == OrderStatus.PaymentReceived)
                               .ToListAsync();
            return list.Count > 0
                ? list
                : null;
        }



        public async Task UpdateTrackingInfo(string orderId, OrderStatus status, ShippingCarrier carrier, string trackingNumber)
        {
            var order = await _db.Orders
                                .Where(o => o.OrderId == orderId)
                                .FirstOrDefaultAsync();

            if (order is not null)
            {
                order.Status = status;
                order.Carrier = carrier;
                order.TrackingNumber = trackingNumber;
                order.LastUpdated = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // update the telegram API with the notification to the user now
            }
            else
            {
                // log error here
            }
        }
    }
}
