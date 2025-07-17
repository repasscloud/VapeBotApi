using System.ComponentModel.DataAnnotations;
using NanoidDotNet;

namespace VapeBotApi.Models
{
    public enum OrderStatus
    {
        // Initial
        New = 100,
        ItemsAdded = 101,
        CheckoutRequested = 102,
        CarrierSelected = 110,
        PaymentMethodSet = 120,
        ShippingDetailsRequired = 125,
        ShippingDetailsSaved = 130,

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
        Refunded = 902,
        PartiallyRefunded = 903,
    }
}