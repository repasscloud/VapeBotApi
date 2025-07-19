using System.Text.Json.Serialization;
namespace VapeBotApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public required string OrderId { get; set; }
        public required string ProductId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Navigation
        [JsonIgnore]
        public Order Order { get; set; } = null!;
        [JsonIgnore]
        public Product Product { get; set; } = null!;
    }
}