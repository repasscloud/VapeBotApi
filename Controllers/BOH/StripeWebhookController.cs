using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using VapeBotApi.Services.Interfaces;  // adjust namespace

namespace VapeBotApi.Controllers.BOH
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _webhookSecret;
        private readonly IPaymentService _paymentService;

        public StripeWebhookController(
            IConfiguration config,
            IPaymentService paymentService)
        {
            _webhookSecret = config["Stripe:SigningSecret"]
                ?? throw new ArgumentNullException(
                    nameof(config),
                    "Missing Stripe:SigningSecret in configuration");
            _paymentService = paymentService;
        }

        [HttpPost("webhook"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> Webhook()
        {
            // 1) Read raw JSON body
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            // 2) Grab the Stripe‑Signature header
            var sigHeader = Request.Headers["Stripe-Signature"];

            Event stripeEvent;
            try
            {
                // 3) Verify & parse webhook
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    sigHeader,
                    _webhookSecret
                );
            }
            catch (StripeException e)
            {
                return BadRequest($"⚠️ Signature verification failed: {e.Message}");
            }

            // 4) Dispatch based on EventTypes constants
            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    var session = stripeEvent.Data.Object as Session;
                    if (session is not null)
                    {
                        await _paymentService.ProcessCheckoutSessionAsync(
                            session.PaymentIntentId,
                            session.Metadata["orderRef"]
                        );
                    }
                    break;

                // case EventTypes.PaymentIntentSucceeded:
                //     var pi = stripeEvent.Data.Object as PaymentIntent;
                //     // optional extra handling
                //     break;

                // handle other events if needed…
            }

            // 5) Return a 2xx to Stripe so it won’t retry
            return Ok();
        }
    }
}
