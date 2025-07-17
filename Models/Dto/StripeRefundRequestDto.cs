namespace VapeBotApi.Models.Dto
{
    public class StripeRefundRequestDto
    {
        public string OrderId { get; set; } = default!;
        public decimal? Amount { get; set; }
        public bool? FullRefund { get; set; }
    }
}