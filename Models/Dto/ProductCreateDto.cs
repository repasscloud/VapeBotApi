namespace VapeBotApi.Models.Dto
{
    public class ProductCreateDto
    {
        public required string Name { get; set; }
        public string? Emoji { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}