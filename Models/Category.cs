using System.ComponentModel.DataAnnotations;

namespace VapeBotApi.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; } = 0;
        public required string Name { get; set; }

        // Navigation
        public List<Product> Products { get; set; } = new();
    }
}