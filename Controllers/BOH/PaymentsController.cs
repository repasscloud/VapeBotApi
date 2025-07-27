// Controllers/BOH/PaymentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Models.Dto;
using VapeBotApi.Models.NowPaymentsIO;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;

        public PaymentsController(AppDbContext db, IConfiguration config, IPaymentService paymentService)
        {
            _db = db;
            _config = config;
            _paymentService = paymentService;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] StripeCheckoutRequestDto dto)
        {
            // 1) Build your shipping rate string
            var stripeShippingRate = dto.Carrier switch
            {
                ShippingCarrier.AustPost => "shr_1RlJpnKcwfnufCuk51FY7o6C",
                _ => null
            };

            // 2) Create the Checkout Session (expand the PI so we get its ID back)
            var options = new SessionCreateOptions
            {
                SuccessUrl = "https://yourapp.com/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://yourapp.com/cancel",
                Mode = "payment",
                Expand = new List<string> { "payment_intent" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = dto.Currency,
                            UnitAmount = (long)(dto.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = "Purchase",
                                Description = $"Order {dto.OrderRef}"
                            },
                        },
                        Quantity = 1,
                    }
                },
                ShippingOptions = stripeShippingRate is null
                    ? null
                    : new List<SessionShippingOptionOptions>
                    {
                        new() { ShippingRate = stripeShippingRate }
                    },
                CustomerEmail = dto.Email,
                Metadata = new Dictionary<string, string>
                {
                    ["userChatId"] = dto.UserChatId.ToString(),
                    ["orderRef"] = dto.OrderRef
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // 3) Try to find an existing Order by your OrderRef primary key
            var order = await _db.Orders.FindAsync(dto.OrderRef);

            if (order != null)
            {
                // update the stub you previously created
                order.Total = dto.Amount;
                order.Currency = dto.Currency;
                order.Status = OrderStatus.PaymentPending;
                order.StripePaymentIntentId = session.PaymentIntentId!;
                order.LastUpdated = DateTime.UtcNow;
                _db.Orders.Update(order);
            }
            else
            {
                // create fresh
                order = new Order
                {
                    OrderId = dto.OrderRef,
                    UserChatId = dto.UserChatId,
                    Total = dto.Amount,
                    Currency = dto.Currency,
                    Status = OrderStatus.PaymentPending,
                    StripePaymentIntentId = session.PaymentIntentId!,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _db.Orders.Add(order);
            }

            await _db.SaveChangesAsync();

            // 4) Return the Checkout URL
            return Ok(new { url = session.Url });
        }



        [HttpPost("refund")]
        public async Task<IActionResult> Refund([FromBody] StripeRefundRequestDto dto)
        {
            var order = await _db.Orders
                                 .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);
            if (order == null)
                return NotFound($"Order '{dto.OrderId}' not found.");

            if (string.IsNullOrEmpty(order.StripePaymentIntentId))
                return BadRequest("No payment intent recorded for this order.");

            var opts = new RefundCreateOptions
            {
                PaymentIntent = order.StripePaymentIntentId
            };

            if (dto.Amount.HasValue)
            {
                opts.Amount = (long)(dto.Amount.Value * 100);
            }
            else if (dto.FullRefund == true)
            {
                // nothing to set → full refund
            }
            else
            {
                return BadRequest("Specify either Amount (partial) or FullRefund = true.");
            }

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(opts);

            // update order status
            if (dto.Amount.HasValue)
            {
                order.Status = OrderStatus.PartiallyRefunded;
                order.RefundedAmount = (order.RefundedAmount ?? 0) + dto.Amount.Value;
                order.Total = order.Total - order.RefundedAmount;
            }
            else
            {
                order.Status = OrderStatus.Refunded;
                order.RefundedAmount = order.Total;
                order.Total = 0m;
            }
            await _db.SaveChangesAsync();

            return Ok(refund);
        }

        [HttpGet("nowpayments/external/currencies")]
        public async Task<IActionResult> GetNowPaymentsCurrencies()
        {
            var result = await _paymentService.GetNowPaymentsIOSupportedCurrenciesAsync();
            return Ok(result);
        }

        [HttpGet("nowpayments/currencies")]
        public async Task<ActionResult<List<PaymentCurrencyInfo>>> GetAllNowPaymentsCurrencies()
        {
            var result = await _paymentService.GetAllNowPaymentsIOCurrencyInfosAsync();
            return Ok(result);
        }


        [HttpGet("nowpayments/currencies/{currencyCodeFull}")]
        public async Task<ActionResult<PaymentCurrencyInfo>> GetNowPaymentsCurrency(string currencyCodeFull)
        {
            var result = await _paymentService.GetNowPaymentsIOCurrencyInfoAsync(currencyCodeFull);
            if (result == null)
                return NotFound($"Currency '{currencyCodeFull}' not found.");
            return Ok(result);
        }


        [HttpPost("nowpayments/currencies")]
        public async Task<IActionResult> AddOrUpdateNowPaymentsCurrency([FromBody] PaymentCurrencyInfo info)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _paymentService.AddOrUpdateNowPaymentsIOCurrencyInfoAsync(info);
            return NoContent();
        }


        [HttpDelete("nowpayments/currencies/{currencyCodeFull}")]
        public async Task<IActionResult> DeleteNowPaymentsCurrency(string currencyCodeFull)
        {
            var success = await _paymentService.DeleteNowPaymentsIOCurrencyInfoAsync(currencyCodeFull);
            if (!success)
                return NotFound($"Currency '{currencyCodeFull}' could not be deleted or does not exist.");

            return NoContent();
        }
    }
}
