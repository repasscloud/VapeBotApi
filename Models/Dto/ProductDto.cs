namespace VapeBotApi.Models.Dto
{
    public class ProductDto
    {
        public required string ProductId { get; set; }
        public required string Name { get; set; }
        public string? Emoji { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}