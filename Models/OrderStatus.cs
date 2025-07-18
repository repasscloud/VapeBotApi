namespace VapeBotApi.Models
{
    public enum OrderStatus
    {
        // Initial
        New = 100,

        // Payment (2xx)
        PaymentPending = 200,
        PaymentReceived = 210,
        PaymentFailed = 215,

        // Fulfillment (3xx)
        Packing = 300,
        PendingShipment = 310,
        Shipped = 320,

        // Final (9xx)
        Completed = 900,
        Cancelled = 910,
        Refunded = 920,
        PartiallyRefunded = 930,
    }
}