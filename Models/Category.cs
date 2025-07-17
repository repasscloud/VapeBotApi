namespace VapeBotApi.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        // Navigation
        public List<Product> Products { get; set; } = new();
    }
}