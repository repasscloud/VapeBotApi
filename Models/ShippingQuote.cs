namespace VapeBotApi.Models
{
    public class ShippingQuote
    {
        public int Id { get; set; }
        public required string ServiceName { get; set; }
        public int MaxItems { get; set; }
        public int Capacity { get; set; }
        public decimal Rate { get; set; }
    }
}