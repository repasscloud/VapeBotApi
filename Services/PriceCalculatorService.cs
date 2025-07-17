// PriceCalculatorService.cs
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using VapeBotApi.Services.Interfaces;
using VapeBotApi.Settings;

namespace VapeBotApi.Services
{
    public class PriceCalculatorService : IPriceCalculatorService
    {
        private readonly PriceCalculatorOptions _opts;

        public PriceCalculatorService(IOptions<PriceCalculatorOptions> opts)
        {
            _opts = opts.Value;
        }

        public decimal CalculatePrice(decimal cost, decimal? margin = null, decimal? feeRate = null)
        {
            var m = margin  ?? _opts.DefaultMargin;
            var f = feeRate ?? _opts.DefaultFeeRate;

            // 1) compute raw
            var rawPrice = cost * (1 + m) / (1 - f);

            // 2) round to nearest integer:
            //    .50+. → up,  .01–.49 → down
            var rounded = Math.Round(rawPrice, 0, MidpointRounding.AwayFromZero);

            return rounded;
        }
    }
}
