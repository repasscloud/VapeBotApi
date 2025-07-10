namespace VapeBotApi.Models
{
    public class FaqItem
    {
        public int FaqItemId { get; set; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }
}