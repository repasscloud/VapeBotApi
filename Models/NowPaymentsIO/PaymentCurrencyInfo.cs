namespace VapeBotApi.Models.NowPaymentsIO
{
    public class PaymentCurrencyInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// Human-friendly name of the coin (e.g. "Tether", "USD Coin")
        /// </summary>
        public required string CoinName { get; set; }

        /// <summary>
        /// Full currency code from NowPayments (e.g. "usdttrc20", "usdcop")
        /// </summary>
        public required string CurrencyCodeFull { get; set; }

        /// <summary>
        /// Short currency code (e.g. "usdt", "usdc", "btc")
        /// </summary>
        public required string CurrencyCode { get; set; }

        /// <summary>
        /// Blockchain network (e.g. "TRON", "Ethereum", "Optimism")
        /// </summary>
        public required string Network { get; set; }

        /// <summary>
        /// URL or relative path to a coin icon/image
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}
