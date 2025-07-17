namespace VapeBotApi.Models.Dto
{
    public class StripeCheckoutRequestDto
    {
        public int ItemQty { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "aud";
        public string Email { get; set; } = default!;
        public long UserChatId { get; set; }
        public string OrderRef { get; set; } = default!;
        public ShippingCarrier Carrier { get; set; } = ShippingCarrier.None;
    }
}