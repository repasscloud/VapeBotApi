using NanoidDotNet;

namespace VapeBotApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public required string OrderId { get; set; }
        public required string ProductId { get; set; }
        public int Quantity { get; set; }
        // Navigation
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}