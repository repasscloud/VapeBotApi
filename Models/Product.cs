using System.ComponentModel.DataAnnotations;
using NanoidDotNet;

namespace VapeBotApi.Models
{
    public class Product
    {
        [Key]
        public string ProductId { get; set; } = Nanoid.Generate();
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        // Navigation
        public Category Category { get; set; } = null!;
    }
}