// Models/NowPaymentsIO/CryptoPaymentIntent.cs
namespace VapeBotApi.Models.NowPaymentsIO
{
    public class CryptoPaymentIntent
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = default!;
        public string InvoiceId { get; set; } = default!;
        public string TokenId { get; set; } = default!;
        public string InvoiceUrl { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}