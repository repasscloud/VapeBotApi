using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NanoidDotNet;

namespace VapeBotApi.Models
{
    public class Product
    {
        [Key]
        public string ProductId { get; set; }
            = $"_aprod__{Nanoid.Generate()}";
        
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        // Navigation
        [JsonIgnore]
        public Category Category { get; set; } = null!;
    }
}