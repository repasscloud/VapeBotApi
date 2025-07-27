using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VapeBotApi.Models;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers.BOH
{
    [ApiController]
    [Route("api/paypal/webhook")]
    public class PayPalWebhookController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<PayPalWebhookController> _logger;

        public PayPalWebhookController(IOrderService orderService, ILogger<PayPalWebhookController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            try
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(body);
                var eventType = payload.GetProperty("event_type").GetString();
                var resource = payload.GetProperty("resource");

                if (eventType == "INVOICING.INVOICE.PAID")
                {
                    var invoiceId = resource.GetProperty("id").GetString();
                    var amount = resource.GetProperty("amount").GetProperty("value").GetDecimal();

                    _logger.LogInformation("✅ PayPal invoice paid: {InvoiceId} for {Amount}", invoiceId, amount);

                    var order = await _orderService.GetOrderByPayPalInvoiceIdAsync(invoiceId!);
                    if (order == null)
                    {
                        _logger.LogWarning("⚠️ Order not found for invoice {InvoiceId}", invoiceId);
                        return NotFound();
                    }

                    order.Status = OrderStatus.PaymentReceived;
                    order.PaidAt = DateTime.UtcNow;

                    await _orderService.UpdateOrderAsync(order);

                    return Ok();
                }

                _logger.LogInformation("Unhandled PayPal event: {EventType}", eventType);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing PayPal webhook");
                return StatusCode(500);
            }
        }
    }
}