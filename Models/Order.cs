using System.ComponentModel.DataAnnotations;
using NanoidDotNet;

namespace VapeBotApi.Models
{
    public class Order
    {
        [Key]
        public string OrderId { get; set; } = Nanoid.Generate(alphabet: Nanoid.Alphabets.LettersAndDigits, size: 14);
        public long UserChatId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public OrderPaymentMethod PaymentMethod { get; set; } = OrderPaymentMethod.None;
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address (e.g. you@domain.com).")]
        public string? EmailAddress { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public AUState? State  { get; set; }
        public string? ZipCode { get; set; }
        public string? MobileNo { get; set; }
        public ShippingCarrier Carrier { get; set; } = ShippingCarrier.None;
        public decimal? SubTotal { get; set; }
        public decimal? ShippingFee { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Total { get; set; }
        public decimal? RefundedAmount { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string Currency { get; set; } = "aud";
        public string? StripePaymentIntentId { get; set; }
        public string? StripePaymentUrl { get; set; }
        public string? PayPalInvoiceId { get; set; }
        public string? PayPalPaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<OrderItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Australian States & Territories
    /// </summary>
    public enum AUState
    {
        NSW = 0,  // New South Wales
        QLD = 1,  // Queensland
        VIC = 2,  // Victoria
        TAS = 3,  // Tasmania
        SA = 4,  // South Australia
        WA = 5,  // Western Australia
        NT = 6,  // Northern Territory
        ACT = 7   // Australian Capital Territory
    }

    public enum ShippingCarrier
    {
        None = 0,
        AustPost = 100,
        ExpressPost = 110
    }

    public enum OrderPaymentMethod
    {
        None     =   0,  // “00”

        // External API
        Stripe = 100,
        Crypto = 110,
        PayPal = 120,

        // Cash
        PayID = 200,
        InPerson =  210,
    }
}