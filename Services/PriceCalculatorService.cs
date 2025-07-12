// PriceCalculatorService.cs
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class PriceCalculatorService : IPriceCalculatorService
    {
        // your “preset” defaults
        private const decimal DefaultMargin  = 0.10m;   // 10%
        private const decimal DefaultFeeRate = 0.0505m; // 5.05%

        public decimal CalculatePrice(decimal cost, decimal? margin = null, decimal? feeRate = null)
        {
            var m = margin  ?? DefaultMargin;
            var f = feeRate ?? DefaultFeeRate;

            // 1) compute raw
            var rawPrice = cost * (1 + m) / (1 - f);

            // 2) round to nearest integer:
            //    .50+. → up,  .01–.49 → down
            var rounded = Math.Round(rawPrice, 0, MidpointRounding.AwayFromZero);

            return rounded;
        }
    }
}
