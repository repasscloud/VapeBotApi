using System.ComponentModel.DataAnnotations;

namespace VapeBotApi.Models.Admin
{
    public class DeletedAccount
    {
        [Key]
        public int Id { get; set; }
        public long UserChatId { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime ActionedDate { get; set; }
    }
}