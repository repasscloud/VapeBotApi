using Microsoft.EntityFrameworkCore;
using Stripe;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Models.Dto;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class OrderService : IOrderService
    {
        private const string OrderDetailsUrl = "https://secure-endlessly-puma.ngrok-free.app/OrderDetails?Id=";
        private readonly AppDbContext _db;
        private readonly IPaymentService _pay;
        public OrderService(AppDbContext db, IPaymentService pay)
        {
            _db = db;
            _pay = pay;
        }

        public async Task<string> CreateOrderGetIdAsync(long chatId)
        {
            // 1) Try to fetch an existing pending order *with* its items
            var existing = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o =>
                    o.UserChatId == chatId
                && (o.Status == OrderStatus.New
                || o.Status == OrderStatus.ItemsAdded
                || o.Status == OrderStatus.CheckoutRequested
                || o.Status == OrderStatus.CarrierSelected
                || o.Status == OrderStatus.PaymentPending)
                );

            if (existing != null)
                return existing.OrderId;  // has Items loaded

            // 2) Otherwise, create a fresh one.
            var order = new Order { UserChatId = chatId };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // 3) Its Items list is just the default new List<OrderItem>(),
            //    ready for you to .Add or .Remove without further loading.
            return order.OrderId;
        }

        public async Task<Order> CreateOrderAsync(long chatId)
        {
            // 1) Try to fetch an existing pending order *with* its items
            var existing = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o =>
                    o.UserChatId == chatId
                && (o.Status == OrderStatus.New
                || o.Status == OrderStatus.ItemsAdded
                || o.Status == OrderStatus.CheckoutRequested
                || o.Status == OrderStatus.CarrierSelected
                || o.Status == OrderStatus.PaymentPending)
                );

            if (existing != null)
                return existing;  // has Items loaded

            // 2) Otherwise, create a fresh one.
            var order = new Order { UserChatId = chatId };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // 3) Its Items list is just the default new List<OrderItem>(),
            //    ready for you to .Add or .Remove without further loading.
            return order;
        }

        public async Task AddItemAsync(string orderId, string productId, int quantity)
        {
            var productInfo = await _db.Products
                .Where(p => p.ProductId == productId)
                .FirstOrDefaultAsync();

            if (productInfo != null)
            {
                var item = new OrderItem { OrderId = orderId, ProductId = productId, ProductName = productInfo.Name, Quantity = quantity };
                _db.OrderItems.Add(item);
                await _db.SaveChangesAsync();
            }
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
            AUState auState, string zipCode, string mobileNo, OrderPaymentMethod method,
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
        public Task<List<Order>> GetAllOrdersAsync() =>
            _db.Orders.ToListAsync();

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

        public async Task<bool> AddToCartAsync(long chatId, string productId, int qty)
        {
            var order = await CreateOrderAsync(chatId);

            // 1) Load product and bail out if it doesn't exist
            var productInfo = await _db.Products
                .Where(p => p.ProductId == productId)
                .FirstOrDefaultAsync();

            if (productInfo is null)
            {
                // you could throw, log, or just return false
                return false;
            }

            // 2) See if we already have this in the order
            var item = order.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                // update existing
                item.Quantity += qty;
                item.Price = productInfo.Price * item.Quantity;  // keep price in sync
                _db.OrderItems.Update(item);
            }
            else
            {
                // add new
                var newItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = productId,
                    ProductName = productInfo.Name,
                    Quantity = qty,
                    Price = productInfo.Price * qty,
                };
                await _db.OrderItems.AddAsync(newItem);
            }

            order.Status = OrderStatus.ItemsAdded;
            order.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubtractFromCartAsync(long chatId, string productId, int qty)
        {
            var order = await CreateOrderAsync(chatId);
            if (order == null)
                return false;

            // 1) Load product and bail if it doesn't exist
            var productInfo = await _db.Products
                .Where(p => p.ProductId == productId)
                .FirstOrDefaultAsync();
            if (productInfo == null)
                return false;

            // 2) Find the existing cart item
            var item = order.Items
                .FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return false;

            // 3) Subtract or remove
            if (item.Quantity > qty)
            {
                item.Quantity -= qty;
                item.Price = productInfo.Price * item.Quantity;  // keep price in sync
                _db.OrderItems.Update(item);
            }
            else
            {
                _db.OrderItems.Remove(item);
            }

            order.Status = OrderStatus.ItemsAdded;
            order.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmptyCartAsync(long chatId)
        {
            var order = await CreateOrderAsync(chatId);

            if (order == null)
                return false;

            order.Status = OrderStatus.Canceled;
            order.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> RequestCheckoutAsync(long chatId)
        {
            var order = await CreateOrderAsync(chatId);
            if (order == null)
                return 0.00m;

            // mark as checkout requested
            order.Status = OrderStatus.CheckoutRequested;
            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // sum up all item prices
            var total = order.Items.Sum(i => i.Price);
            return total;
        }

        public async Task<decimal> SetShippingCarrierAsync(long chatId, string carrierCode)
        {
            // 1) grab the order
            var orderId = await CreateOrderGetIdAsync(chatId);
            if (orderId == null)
                return 0.00m;

            var order = await CreateOrderAsync(chatId);
            if (order == null)
                return 0.00m;

            var shippingOptions = await GetShippingOptionsAsync(chatId);

            var match = shippingOptions
                .FirstOrDefault(o => o.Service == carrierCode)
                ?? throw new InvalidOperationException($"No shipping option for {carrierCode}");

            decimal shippingPrice = match.Price;

            var (carrier, shippingFee) = carrierCode.Trim().ToLowerInvariant() switch
            {
                "parcelpost" => (ShippingCarrier.AustPost, shippingPrice),
                "expresspost" => (ShippingCarrier.ExpressPost, shippingPrice),
                _ => (ShippingCarrier.None, 0.00m)
            };

            order.Carrier = carrier;
            order.Status = OrderStatus.CarrierSelected;
            order.LastUpdated = DateTime.UtcNow;

            // 4) compute amounts
            var itemsTotal = order.Items.Sum(i => i.Price);
            const decimal gstRate = 0.10m;  // 10% GST

            // breakup items
            var netItems = itemsTotal / (1 + gstRate);
            var taxItems = itemsTotal - netItems;

            // breakup shipping
            var netShipping = shippingFee / (1 + gstRate);
            var taxShipping = shippingFee - netShipping;

            // assign into your order
            order.SubTotal = decimal.Round(netItems, 2);
            order.ShippingFee = decimal.Round(netShipping, 2);
            order.Tax = decimal.Round(taxItems + taxShipping, 2);

            // Total stays the original gross sum
            order.Total = itemsTotal + shippingFee;

            // commit
            await _db.SaveChangesAsync();
            return (decimal)order.Total;
        }

        public async Task<string?> SetPaymentMethodAsync(long chatId, string paymentMethod)
        {
            var order = await CreateOrderAsync(chatId);
            order.Status = OrderStatus.PaymentMethodSet;
            order.LastUpdated = DateTime.UtcNow;
            OrderPaymentMethod method = paymentMethod switch
            {
                "Stripe" => OrderPaymentMethod.Stripe,
                "Crypto" => OrderPaymentMethod.Crypto,
                "PayPal" => OrderPaymentMethod.PayPal,
                "PayID" => OrderPaymentMethod.PayID,
                "InPerson" => OrderPaymentMethod.InPerson,
                _ => OrderPaymentMethod.None
            };

            if (method == OrderPaymentMethod.None)
                return "no payment method";

            order.PaymentMethod = method;
            await _db.SaveChangesAsync();

            // generate payment link
            switch (paymentMethod)
            {
                case "Stripe":
                    string paymentLink = await _pay.GetStripePaymentLink(order.OrderId);
                    return paymentLink;
                default:
                    return "error in switch";
            }
        }

        public async Task<IEnumerable<ShippingOptionDto>> GetShippingOptionsAsync(long chatId)
        {
            var orderId = await CreateOrderGetIdAsync(chatId);
            var order = await GetOrderAsync(orderId);
            var totalItems = order.Items?.Sum(item => item.Quantity) ?? 0;
            var quotes = await _db.ShippingQuotes.ToListAsync();
            return quotes
                .Where(q => totalItems < q.MaxItems)
                .Select(q =>
                {
                    var shipments = (totalItems + q.Capacity - 1) / q.Capacity;
                    return new ShippingOptionDto
                    {
                        Service = q.ServiceName,
                        Price = shipments * q.Rate
                    };
                });
        }

        // public async Task<string?> GetAccountLinkAsync(long chatId)
        // {
        //     var user = await _db.Users.FirstOrDefaultAsync(i => i.ChatId == chatId);
        //     if (user == null)
        //     {
        //         var orderId = await CreateOrderGetIdAsync(chatId);
        //         if (orderId is not null)
        //         {
        //             // set the order to Status=124 (shipping details required)
        //             var order = await GetOrderAsync(orderId);
        //             order.Status = OrderStatus.ShippingDetailsRequired;
        //             await _db.SaveChangesAsync();

        //             return $"{OrderDetailsUrl}{orderId}";
        //         }

        //         // save the record to a log, there is something wrong here!
        //         return null;
        //     }

        //     else if (string.IsNullOrWhiteSpace(user.SavedZipCode))
        //     {
        //         var orderId = await CreateOrderGetIdAsync(chatId);
        //         if (orderId is not null)
        //         {
        //             // set the order Status=124 (shipping details required)
        //             var order = await GetOrderAsync(orderId);
        //             order.Status = OrderStatus.ShippingDetailsRequired;
        //             await _db.SaveChangesAsync();

        //             return $"{OrderDetailsUrl}{orderId}";
        //         }

        //         // save the record to a log, there is something wrong here!
        //         return null;
        //     }

        //     else
        //     {
        //         // shipping details are already on file, does not require me to do anything from here
        //         var orderId = await CreateOrderGetIdAsync(chatId);
        //         var order = await GetOrderAsync(orderId);
        //         order.Status = OrderStatus.ShippingDetailsSaved;
        //         await _db.SaveChangesAsync();
        //         return null;
        //     }
        // }

        public async Task<bool> UpdateShippingDetailsAsync(Order order)
        {
            var existing = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (existing == null)
                return false;

            existing.FirstName = order.FirstName;
            existing.SecondName = order.SecondName;
            existing.AddressLine1 = order.AddressLine1;
            existing.AddressLine2 = order.AddressLine2;
            existing.AddressLine3 = order.AddressLine3;
            existing.State = order.State;
            existing.ZipCode = order.ZipCode;
            existing.MobileNo = order.MobileNo;
            existing.Status = OrderStatus.ShippingDetailsSaved;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
