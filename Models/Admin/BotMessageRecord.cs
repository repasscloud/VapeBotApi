namespace VapeBotApi.Models.Admin
{
    public class BotMessageRecord
    {
        public int Id { get; set; } = 0;
        public long ChatId { get; set; }
        public int    MessageId { get; set; }
        public bool   IsDeleted { get; set; }
        public DateTime SentAt  { get; set; } = DateTime.UtcNow;
    }
}
