// PriceCalculatorOptions.cs
namespace VapeBotApi.Settings
{
    public class PriceCalculatorOptions
    {
        /// <summary>Default net profit margin (e.g. 0.10 for 10%)</summary>
        public decimal DefaultMargin { get; set; }

        /// <summary>Default platform fee rate (e.g. 0.0505 for 5.05%)</summary>
        public decimal DefaultFeeRate { get; set; }
    }
}
