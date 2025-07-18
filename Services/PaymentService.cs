using Microsoft.EntityFrameworkCore;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Services.Interfaces;
using Stripe.Checkout;

namespace VapeBotApi.Services
{
    public class PaymentService : IPaymentService
    {
        private const string AppBaseUrl = "https://secure-endlessly-puma.ngrok-free.app";
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public PaymentService(AppDbContext db, IConfiguration config)
        {
            _db     = db;
            _config = config;
        }

        public async Task<string> GetStripePaymentLink(string orderId)
        {
            // retrieve the order
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order is null)
                return "order is null";

            long shippingInCents = 0;
            if (order.ShippingFee is not null)
                shippingInCents = (long)(order.ShippingFee * 100);
            else
                return "shippingInCents empty";

            long orderInCents = 0;
            if (order.SubTotal is not null)
                orderInCents = (long)(order.SubTotal * 100);
            else
                return "orderInCents empty";

            string shippingText = string.Empty;
            string shippingSubText = string.Empty;
            switch (order.Carrier)
            {
                case ShippingCarrier.AustPost:
                    shippingText = "AustPost ParcelPost";
                    shippingSubText = "3-5 days delivery";
                    break;
                case ShippingCarrier.ExpressPost:
                    shippingText = "AustPost ExpressPost";
                    shippingSubText = "next business day delivery";
                    break;
                default:
                    return "incorrect shipping";
            }

            var sessionOpts = new SessionCreateOptions
            {
                SuccessUrl = $"{AppBaseUrl}/PaymentReceived",
                CancelUrl = $"{AppBaseUrl}/PaymentCancelled",
                Mode = "payment",

                Expand = new List<string> { "payment_intent" },

                // product
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = order.Currency,
                            UnitAmount  = orderInCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = "Purchase",
                                Description = $"Order: {order.OrderId}"
                            },
                        },
                        Quantity = 1,
                    },

                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency    = order.Currency,
                            UnitAmount  = shippingInCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = shippingText,
                                Description = shippingSubText,
                            },
                        },
                        Quantity = 1,
                    }
                },

                CustomerEmail = order.EmailAddress,
                Metadata = new Dictionary<string, string>
                {
                    ["userChatId"] = order.UserChatId.ToString(),
                    ["orderRef"] = order.OrderId
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOpts);

            order.Status = OrderStatus.PaymentPending;
            order.StripePaymentIntentId = session.PaymentIntentId!;
            order.StripePaymentUrl = session.Url;
            order.LastUpdated = DateTime.UtcNow;
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();

            return session.Url;
        }
    }
}
