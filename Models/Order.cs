using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NanoidDotNet;

namespace VapeBotApi.Models
{
    public class Order
    {
        [Key]
        public string OrderId { get; set; } = Nanoid.Generate();
        public long UserChatId { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public AUState? State  { get; set; }
        public string? ZipCode { get; set; }
        public string? MobileNo { get; set; }
        public ShippingCarrier Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; }

        // Navigation
        public List<OrderItem> Items { get; set; } = new();
        public User User { get; set; } = null!;
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
        AustPost = 0,
        ExpressPost = 1
    }

    public enum OrderStatus
    {
        // Initial
        New = 100,

        // Payment (2xx)
        PaymentPending = 200,
        PaymentReceived = 201,

        // Fulfillment (3xx)
        Packing = 300,
        PendingShipment = 301,
        Shipped = 302,

        // Final (9xx)
        Completed = 900,
        Canceled = 901,
        Refunded = 902
    }

    public enum PaymentMethod
    {
        None     =   0,  // “00”

        // External API
        Stripe = 10,
        Crypto = 11,
        PayPal = 12,

        // Cash
        PayID = 20,
        InPerson =  21,
    }
}