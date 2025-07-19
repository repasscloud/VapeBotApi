using Microsoft.EntityFrameworkCore;
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

        #region create_order
        public async Task<string?> GetCurrentNewOrderFromChatIdAsync(long chatId)
        {
            var existingOrder = await _db.Orders
                .FirstOrDefaultAsync(o =>
                    o.UserChatId == chatId
                    && o.Status == OrderStatus.New);

            return existingOrder == null ? null : existingOrder.OrderId;
        }

        public async Task<string> GenerateNewOrderFromChatIdAsync(long chatId)
        {
            var orderId = await GetCurrentNewOrderFromChatIdAsync(chatId);

            if (orderId is not null)
            {
                return orderId;
            }
            else
            {
                var order = new Order { UserChatId = chatId };
                await _db.Orders.AddAsync(order);
                await _db.SaveChangesAsync();
                return order.OrderId;
            }
        }

        public async Task<List<CategoryDto>?> GetListCategoryDtoListAsync()
        {
            var entities = await _db.Categories.ToListAsync();

            if (!entities.Any())
                return null;

            var dtos = entities
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })
                .ToList();

            return dtos;
        }

        public async Task<List<ProductDto>?> GetProductDtoListFromCategoryIdAsync(int categoryId)
        {
            var dtos = await _db.Products
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return dtos.Count > 0
                ? dtos
                : null;
        }

        public async Task<bool> AddItemToCurrentNewOrderAsync(long chatId, string productId, int qty)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return false;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (qty <= 0)
                return false;

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product is null)
                return false;

            var existing = order.Items
                .SingleOrDefault(i => i.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += qty;
                existing.Price = existing.Quantity * product.Price;
                _db.OrderItems.Update(existing);
            }
            else
            {
                var newItem = new OrderItem
                {
                    OrderId = orderCode,
                    ProductId = productId,
                    ProductName = product.Name,
                    Quantity = qty,
                    Price = qty * product.Price
                };
                await _db.OrderItems.AddAsync(newItem);
            }

            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemFromCurrentNewOrderAsync(long chatId, string productId, int qty)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return false;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (qty <= 0)
                return false;

            var existing = order.Items
                .SingleOrDefault(i => i.ProductId == productId);
            if (existing is null)
                return false;

            existing.Quantity -= qty;
            if (existing.Quantity <= 0)
            {
                _db.OrderItems.Remove(existing);
            }
            else
            {
                var productPrice = await _db.Products
                    .Where(p => p.ProductId == productId)
                    .Select(p => p.Price)
                    .FirstAsync();
                existing.Price = existing.Quantity * productPrice;
                _db.OrderItems.Update(existing);
            }

            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmptyCurrentNewOrderAsync(long chatId)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return true;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (order == null)
                return true;

            order.Status = OrderStatus.Cancelled;
            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
        #endregion

        #region show_cart
        public async Task<List<OrderItem>?> ShowCurrentNewOrderItemsAsync(long chatId)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return null;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (!order.Items.Any())
                return null;

            return order.Items.ToList();
        }
        #endregion

        #region checkout_order
        public async Task<decimal?> RequestCheckoutAsync(long chatId)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return null;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (!order.Items.Any())
                return null;

            var total = order.Items.Sum(i => i.Price);
            return total;
        }

        public async Task<List<ShippingOptionDto>?> GetShippingOptionsAsync(long chatId)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return null;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (!order.Items.Any())
                return null;

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
                })
                .ToList();
        }

        public async Task<bool> SetShippingCarrierAsync(long chatId, string carrierCode)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return false;

            var shippingOptions = await GetShippingOptionsAsync(chatId);
            if (shippingOptions is null)
                return false;

            var match = shippingOptions
                .FirstOrDefault(o => o.Service == carrierCode);
            if (match is null)
                return false;

            decimal shippingPrice = match.Price;

            var (carrier, shippingFee) = carrierCode.Trim().ToLowerInvariant() switch
            {
                "parcelpost" => (ShippingCarrier.AustPost, shippingPrice),
                "expresspost" => (ShippingCarrier.ExpressPost, shippingPrice),
                _ => (ShippingCarrier.None, 0.00m)
            };

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (!order.Items.Any())
                return false;

            order.Carrier = carrier;
            order.LastUpdated = DateTime.UtcNow;

            // compute amounts
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
            return true;
        }

        public async Task<string?> SetPaymentMethodAsync(long chatId, string paymentMethod)
        {
            string? orderCode = await GetCurrentNewOrderFromChatIdAsync(chatId);
            if (orderCode is null)
                return null;

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == orderCode);

            if (!order.Items.Any())
                return null;

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
                return null;

            order.PaymentMethod = method;
            order.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return "payment page link goes here";
        }
        #endregion

        #region webapp
        public async Task<string> FinalizeOrderAsync(Order update)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == update.OrderId);
            if (order is null)
                return "https://www.google.com.au/";

            order.FirstName = update.FirstName;
            order.SecondName = update.SecondName;
            order.MobileNo = update.MobileNo;
            order.AddressLine1 = update.AddressLine1;
            order.AddressLine2 = update.AddressLine2;
            order.AddressLine3 = update.AddressLine3;
            order.EmailAddress = update.EmailAddress;
            order.State = update.State;
            order.ZipCode = update.ZipCode;
            order.Status = OrderStatus.PaymentPending;
            order.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            switch (order.PaymentMethod)
            {
                case OrderPaymentMethod.Stripe:
                    var url = await _pay.GetStripePaymentLink(order.OrderId);
                    return url;
                default:
                    return "https://www.google.com.au/";
            }

        }
        #endregion


























        // public async Task<string> CreateOrderGetIdAsync(long chatId)
        // {
        //     // 1) Try to fetch an existing pending order *with* its items
        //     var existing = await _db.Orders
        //         .Include(o => o.Items)
        //         .FirstOrDefaultAsync(o =>
        //             o.UserChatId == chatId
        //             && (o.Status == OrderStatus.New
        //             || o.Status == OrderStatus.PaymentPending)
        //         );

        //     if (existing != null)
        //         return existing.OrderId;  // has Items loaded

        //     // 2) Otherwise, create a fresh one.
        //     var order = new Order { UserChatId = chatId };
        //     _db.Orders.Add(order);
        //     await _db.SaveChangesAsync();

        //     // 3) Its Items list is just the default new List<OrderItem>(),
        //     //    ready for you to .Add or .Remove without further loading.
        //     return order.OrderId;
        // }

        // public async Task<Order> CreateOrderAsync(long chatId)
        // {
        //     // 1) Try to fetch an existing pending order *with* its items
        //     var existing = await _db.Orders
        //         .Include(o => o.Items)
        //         .FirstOrDefaultAsync(o =>
        //             o.UserChatId == chatId
        //             && (o.Status == OrderStatus.New
        //             || o.Status == OrderStatus.PaymentPending)
        //         );

        //     if (existing != null)
        //         return existing;  // has Items loaded

        //     // 2) Otherwise, create a fresh one.
        //     var order = new Order { UserChatId = chatId };
        //     _db.Orders.Add(order);
        //     await _db.SaveChangesAsync();

        //     // 3) Its Items list is just the default new List<OrderItem>(),
        //     //    ready for you to .Add or .Remove without further loading.
        //     return order;
        // }

        // public async Task AddItemAsync(string orderId, string productId, int quantity)
        // {
        //     var productInfo = await _db.Products
        //         .Where(p => p.ProductId == productId)
        //         .FirstOrDefaultAsync();

        //     if (productInfo != null)
        //     {
        //         var item = new OrderItem { OrderId = orderId, ProductId = productId, ProductName = productInfo.Name, Quantity = quantity };
        //         _db.OrderItems.Add(item);
        //         await _db.SaveChangesAsync();
        //     }
        // }

        // public async Task<Order> GetOrderAsync(string orderId)
        // {
        //     var order = await _db.Orders
        //         .Include(o => o.Items)
        //         .ThenInclude(i => i.Product)
        //         .FirstOrDefaultAsync(o => o.OrderId == orderId);

        //     if (order == null)
        //         throw new KeyNotFoundException($"Order {orderId} not found.");

        //     return order;
        // }

        // public Task<List<Order>> GetUserOrdersAsync(long chatId) => _db.Orders
        //     .Where(o => o.UserChatId == chatId)
        //     .Include(o => o.Items).ThenInclude(i => i.Product)
        //     .ToListAsync();

        // public async Task FinalizeOrderAsync(
        //     string orderId, string firstName, string secondName, string addressLine1,
        //     AUState auState, string zipCode, string mobileNo, OrderPaymentMethod method,
        //     string? addressLine2, string? addressLine3)
        // {
        //     var order = await _db.Orders.FindAsync(orderId);
        //     if (order == null) throw new InvalidOperationException();

        //     order.FirstName = firstName;
        //     order.SecondName = secondName;
        //     order.AddressLine1 = addressLine1;
        //     order.State = auState;
        //     order.ZipCode = zipCode;
        //     order.MobileNo = mobileNo;

        //     if (!string.IsNullOrWhiteSpace(addressLine2))
        //     {
        //         order.AddressLine2 = addressLine2;
        //     }

        //     if (!string.IsNullOrWhiteSpace(addressLine3))
        //     {
        //         order.AddressLine3 = addressLine3;
        //     }

        //     order.PaymentMethod = method;
        //     order.Status = OrderStatus.PaymentPending;
        //     order.LastUpdated = DateTime.UtcNow;
        //     await _db.SaveChangesAsync();
        // }

        // public async Task CancelOrderAsync(string orderId)
        // {
        //     var order = await _db.Orders.FindAsync(orderId);
        //     if (order == null) throw new InvalidOperationException();
        //     order.Status = OrderStatus.Cancelled;
        //     order.LastUpdated = DateTime.UtcNow;
        //     await _db.SaveChangesAsync();
        // }

        // // admin
        // public Task<List<Order>> GetAllOrdersAsync() =>
        //     _db.Orders.ToListAsync();

        // public async Task UpdateOrderPaymentReceivedAsync(string orderId)
        // {
        //     var order = await _db.Orders
        //                         .FindAsync(orderId);
        //     if (order is not null)
        //     {
        //         order.Status = OrderStatus.PaymentReceived;
        //         order.LastUpdated = DateTime.UtcNow;
        //         await _db.SaveChangesAsync();

        //         // update the telegram API with the notification to the user now
        //     }
        //     else
        //     {
        //         // log error here
        //     }
        // }

        // public async Task<List<Order>?> GetPaymentReceivedOrdersAsync()
        // {
        //     var list = await _db.Orders
        //                 .Where(o => o.Status == OrderStatus.PaymentReceived)
        //                 .ToListAsync();
        //     return list.Count > 0
        //         ? list
        //         : null;
        // }



        // public async Task<bool> AddToCartAsync(long chatId, string productId, int qty)
        // {
        //     var order = await CreateOrderAsync(chatId);

        //     // 1) Load product and bail out if it doesn't exist
        //     var productInfo = await _db.Products
        //         .Where(p => p.ProductId == productId)
        //         .FirstOrDefaultAsync();

        //     if (productInfo is null)
        //     {
        //         // you could throw, log, or just return false
        //         return false;
        //     }

        //     // 2) See if we already have this in the order
        //     var item = order.Items.FirstOrDefault(i => i.ProductId == productId);

        //     if (item != null)
        //     {
        //         // update existing
        //         item.Quantity += qty;
        //         item.Price = productInfo.Price * item.Quantity;  // keep price in sync
        //         _db.OrderItems.Update(item);
        //     }
        //     else
        //     {
        //         // add new
        //         var newItem = new OrderItem
        //         {
        //             OrderId = order.OrderId,
        //             ProductId = productId,
        //             ProductName = productInfo.Name,
        //             Quantity = qty,
        //             Price = productInfo.Price * qty,
        //         };
        //         await _db.OrderItems.AddAsync(newItem);
        //     }

        //     order.LastUpdated = DateTime.UtcNow;

        //     await _db.SaveChangesAsync();
        //     return true;
        // }

        // public async Task<bool> SubtractFromCartAsync(long chatId, string productId, int qty)
        // {
        //     var order = await CreateOrderAsync(chatId);
        //     if (order == null)
        //         return false;

        //     // 1) Load product and bail if it doesn't exist
        //     var productInfo = await _db.Products
        //         .Where(p => p.ProductId == productId)
        //         .FirstOrDefaultAsync();
        //     if (productInfo == null)
        //         return false;

        //     // 2) Find the existing cart item
        //     var item = order.Items
        //         .FirstOrDefault(i => i.ProductId == productId);
        //     if (item == null)
        //         return false;

        //     // 3) Subtract or remove
        //     if (item.Quantity > qty)
        //     {
        //         item.Quantity -= qty;
        //         item.Price = productInfo.Price * item.Quantity;  // keep price in sync
        //         _db.OrderItems.Update(item);
        //     }
        //     else
        //     {
        //         _db.OrderItems.Remove(item);
        //     }

        //     order.LastUpdated = DateTime.UtcNow;

        //     await _db.SaveChangesAsync();
        //     return true;
        // }

        // public async Task<bool> EmptyCartAsync(long chatId)
        // {
        //     var order = await CreateOrderAsync(chatId);

        //     if (order == null)
        //         return false;

        //     order.Status = OrderStatus.Cancelled;
        //     order.LastUpdated = DateTime.UtcNow;

        //     await _db.SaveChangesAsync();
        //     return true;
        // }









        // public async Task<bool> UpdateShippingDetailsAsync(Order order)
        // {
        //     var existing = await _db.Orders
        //         .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

        //     if (existing == null)
        //         return false;

        //     existing.FirstName = order.FirstName;
        //     existing.SecondName = order.SecondName;
        //     existing.AddressLine1 = order.AddressLine1;
        //     existing.AddressLine2 = order.AddressLine2;
        //     existing.AddressLine3 = order.AddressLine3;
        //     existing.State = order.State;
        //     existing.ZipCode = order.ZipCode;
        //     existing.MobileNo = order.MobileNo;

        //     await _db.SaveChangesAsync();
        //     return true;
        // }
    }
}
