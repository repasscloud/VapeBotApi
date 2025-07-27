using System.Text.Json.Serialization;

namespace VapeBotApi.Models.NowPaymentsIO
{
    public class SupportedCurrenciesResponse
    {
        [JsonPropertyName("currencies")]
        public List<SupportedCurrency>? Currencies { get; set; }
    }
    public class SupportedCurrency
    {
        [JsonPropertyName("min_amount")]
        public double MinAmount { get; set; }

        [JsonPropertyName("max_amount")]
        public double MaxAmount { get; set; }

        [JsonPropertyName("currency")]
        public required string Currency { get; set; }
    }
}