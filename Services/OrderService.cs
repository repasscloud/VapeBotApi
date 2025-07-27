using Microsoft.EntityFrameworkCore;
using Stripe;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Models.Admin;
using VapeBotApi.Models.Dto;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class OrderService : IOrderService
    {
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

            return $"{WebAppBase.Url}/OrderDetails?Id={order.OrderId}";
            //"https://secure-endlessly-puma.ngrok-free.app/test-webapp.html";
        }

        public async Task<List<Order>?> GetOrdersTrackingAsync(long chatId)
        {
            var shippedOrders = await _db.Orders
                .Where(o =>
                    o.UserChatId == chatId
                    && !string.IsNullOrEmpty(o.TrackingNumber)
                )
                .ToListAsync();

            if (shippedOrders is null)
                return null;

            return shippedOrders;
        }

        public async Task<List<Order>?> GetOrdersHistoryAsync(long chatId)
        {
            var historyOrders = await _db.Orders
                .Where(o => o.UserChatId == chatId)
                .ToListAsync();

            if (historyOrders is null)
                return null;

            return historyOrders;
        }
        #endregion

        #region webapp
        public async Task<Order?> GetWebAppOrderAsync(string orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return null;

            return order;
        }

        public async Task<string> FinalizeWebAppOrderAsync(Order update)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.OrderId == update.OrderId);
            if (order is null)
                return $"{WebAppBase.NullOrderIdUrl}";

            order.FirstName = update.FirstName;
            order.SecondName = update.SecondName;
            order.MobileNo = update.MobileNo;
            order.AddressLine1 = update.AddressLine1;
            order.AddressLine2 = update.AddressLine2;
            order.AddressLine3 = update.AddressLine3;
            order.EmailAddress = update.EmailAddress;
            order.State = update.State;
            order.ZipCode = update.ZipCode;
            // order.Status = OrderStatus.PaymentPending;  <- must happen AFTER the payment link is generated or it bombs the order out and user can't change it LOL
            order.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            string redirectUrl;

            switch (order.PaymentMethod)
            {
                case OrderPaymentMethod.Stripe:
                    redirectUrl = await _pay.GetStripePaymentLink(order.OrderId);
                    if (!redirectUrl.StartsWith("https://"))
                    {
                        redirectUrl = WebAppBase.StripePaymentGenerationErrorUrl;
                    }
                    break;
                case OrderPaymentMethod.PayPal:
                    var paypalInvoiceUrl = await _pay.GetPayPalInvoiceAsync(order.EmailAddress!, order.OrderId, (decimal)(order.Total! * 100));
                    if (string.IsNullOrWhiteSpace(paypalInvoiceUrl))
                    {
                        redirectUrl = WebAppBase.StripePaymentGenerationErrorUrl;
                    }
                    else
                    {
                        redirectUrl = paypalInvoiceUrl;
                    }
                    break;
                case OrderPaymentMethod.Crypto:
                    redirectUrl = await _pay.GetNowPaymentsIOPaymentLinkAsync(order.OrderId);
                    break;
                default:
                    redirectUrl = "";
                    break;
            }

            return redirectUrl;
        }


        public async Task<bool> CancelOrderAsync(string orderId)
        {
            var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (order is null)
                return false;

            if (order.Status == OrderStatus.New
                || order.Status == OrderStatus.PaymentPending
                || order.Status == OrderStatus.PaymentFailed)
            {
                order.Status = OrderStatus.Cancelled;
                await _db.SaveChangesAsync();
                return true;
            }
            if (order.Status == OrderStatus.PaymentReceived)
            {
                order.Status = OrderStatus.Refunded;

                switch (order.PaymentMethod)
                {
                    case OrderPaymentMethod.Stripe:
                        var adminMsg = new CustomerMessage
                        {
                            Id = 0,
                            ChatId = order.UserChatId,
                            Message = $"[SYSTEM] Process Stripe payment refun for order {order.OrderId}",
                            Created = DateTime.UtcNow
                        };
                        await _db.CustomerMessages.AddAsync(adminMsg);
                        break;

                    case OrderPaymentMethod.Crypto:
                        break;
                    case OrderPaymentMethod.PayPal:
                        break;
                    case OrderPaymentMethod.PayID:
                        break;
                    case OrderPaymentMethod.InPerson:
                        break;
                    case OrderPaymentMethod.None:
                        break;
                    default:
                        order.Status = OrderStatus.Cancelled;
                        break;
                }
            }
            else
            {
                return false;
            }

            await _db.SaveChangesAsync();
            return true;
        }
        #endregion

        public async Task<Order?> GetOrderByPayPalInvoiceIdAsync(string invoiceId)
        {
            var order = await _db.Orders.Where(o => o.PayPalInvoiceId == invoiceId).FirstOrDefaultAsync();
            if (order is null)
                return null;
            return order;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            if (order is null)
                return;

            var matchOrder = await _db.Orders
                .Where(o => o.OrderId == order.OrderId)
                .FirstOrDefaultAsync();

            if (matchOrder is null)
                return;

            matchOrder.Status = order.Status;
            matchOrder.PaidAt = order.PaidAt;
            matchOrder.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
