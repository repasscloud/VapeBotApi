namespace VapeBotApi.Models.Dto
{
    public class ShippingOptionDto
    {
        public required string Service { get; set; }
        public decimal Price { get; set; }
    }
}