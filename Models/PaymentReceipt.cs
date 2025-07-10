using NanoidDotNet;

namespace VapeBotApi.Models
{
    public class PaymentReceipt
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = Nanoid.Generate();
        public PaymentMethod Provider { get; set; }  // Stripe, PayID, etc.
        public required string Reference { get; set; }  // CheckoutId or PaymentId
        public DateTime ReceivedAt { get; set; }
        public string? Metadata { get; set; }

        public Order Order { get; set; } = null!;
    }
}
