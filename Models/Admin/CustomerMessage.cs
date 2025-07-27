using System.ComponentModel.DataAnnotations;
namespace VapeBotApi.Models.Admin
{
    public class CustomerMessage
    {
        [Key]
        public int Id { get; set; }
        public required long ChatId { get; set; }
        public required string Message { get; set; }
        public string? Respone { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Responded { get; set; }
    }
}