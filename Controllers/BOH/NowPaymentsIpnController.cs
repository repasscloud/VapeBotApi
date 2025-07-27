using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VapeBotApi.Models.Admin;
using VapeBotApi.Models.NowPaymentsIO;

namespace VapeBotApi.Controllers.BOH
{
    [ApiController]
    [Route("api/nowpayments")]
    public class NowPaymentsIpnController : ControllerBase
    {
        private readonly ILogger<NowPaymentsIpnController> _logger;
        private readonly NowPaymentsSettings _npSettings;

        public NowPaymentsIpnController(
            ILogger<NowPaymentsIpnController> logger,
            IOptions<NowPaymentsSettings> nowpayments)
        {
            _logger = logger;
            _npSettings = nowpayments.Value;
        }

        [HttpPost("ipn"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReceiveIpn()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var signatureHeader = Request.Headers["x-nowpayments-sig"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                _logger.LogWarning("IPN missing signature header");
                return BadRequest("Missing signature header");
            }

            var ipnSecret = _npSettings.IpnKey;
            if (string.IsNullOrWhiteSpace(ipnSecret))
            {
                _logger.LogError("IPN secret key is not configured");
                return StatusCode(500, "IPN secret is not configured");
            }

            var isValid = ValidateHmacSignature(body, signatureHeader, ipnSecret);
            if (!isValid)
            {
                _logger.LogWarning("IPN signature validation failed");
                return Unauthorized("Invalid signature");
            }

            try
            {
                var payload = JsonSerializer.Deserialize<NowPaymentsIpnPayload>(body);
                if (payload == null)
                    return BadRequest("Invalid payload");

                // TODO: Handle the payload (e.g., update payment record in DB)
                _logger.LogInformation("✅ Valid IPN received: {OrderId}, Status: {PaymentStatus}", payload.order_id, payload.payment_status);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize IPN payload");
                return BadRequest("Deserialization error");
            }
        }

        private static bool ValidateHmacSignature(string body, string signature, string secretKey)
        {
            var encoding = Encoding.UTF8;
            var key = encoding.GetBytes(secretKey);
            using var hmac = new HMACSHA512(key);
            var bodyBytes = encoding.GetBytes(body);
            var hashBytes = hmac.ComputeHash(bodyBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            return hash == signature.ToLowerInvariant();
        }
    }
}
