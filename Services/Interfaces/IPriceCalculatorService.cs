// IPriceCalculatorService.cs
namespace VapeBotApi.Services.Interfaces
{
    public interface IPriceCalculatorService
    {
        /// <summary>
        /// Calculate the gross price from your cost,
        /// with an optional net profit margin and platform fee rate.
        /// Rounds final result to a whole number: 
        ///   .50–.99 → up,  .01–.49 → down.
        /// </summary>
        /// <param name="cost">Your cost price.</param>
        /// <param name="margin">
        /// Desired net profit margin (e.g. 0.10 for 10%). 
        /// Defaults to 10% if null.
        /// </param>
        /// <param name="feeRate">
        /// Platform fee rate (e.g. 0.0505 for 5.05%). 
        /// Defaults to 5.05% if null.
        /// </param>
        decimal CalculatePrice(
            decimal cost,
            decimal? margin = null,
            decimal? feeRate = null
        );
    }
}
