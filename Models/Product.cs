using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NanoidDotNet;
namespace VapeBotApi.Models
{
    public class Product
    {
        [Key]
        public string ProductId { get; set; } = $"{Nanoid.Generate(alphabet: Nanoid.Alphabets.LowercaseLettersAndDigits, size: 10)}";
        public required string Name { get; set; }
        public string? Emoji { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        // Navigation
        [JsonIgnore]
        public Category Category { get; set; } = null!;
    }
}