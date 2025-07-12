using Microsoft.AspNetCore.Mvc;

namespace VapeBotApi.Controllers.Admin
{
    // DTOs
    public class PriceCalculationRequest
    {
        /// <summary>Cost to you</summary>
        public decimal Cost { get; set; }

        /// <summary>Platform fee rate (e.g. 0.0505 for 5.05%)</summary>
        public decimal FeeRate { get; set; }

        /// <summary>Target net profit margin (e.g. 0.10 for 10%)</summary>
        public decimal Margin { get; set; }
    }

    public class PriceCalculationResponse
    {
        /// <summary>Gross price you should charge</summary>
        public decimal Price { get; set; }
    }

    // Controller
    [ApiController]
    [Route("api/admin/calculator")]
    public class PriceCalculatorController : ControllerBase
    {
        [HttpPost("calculate")]
        public ActionResult<PriceCalculationResponse> Calculate([FromBody] PriceCalculationRequest req)
        {
            // price = cost × (1 + margin) ÷ (1 − feeRate)
            var rawPrice = req.Cost * (1 + req.Margin) / (1 - req.FeeRate);

            // round to 2 decimals
            var rounded = Math.Round(rawPrice, 2, MidpointRounding.AwayFromZero);

            return Ok(new PriceCalculationResponse { Price = rounded });
        }
    }
}
