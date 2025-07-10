using System.ComponentModel.DataAnnotations;

namespace VapeBotApi.Models
{
    public class User
    {
        [Key]
        public long ChatId { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? SavedFirstName { get; set; }
        public string? SavedSecondName { get; set; }
        public string? SavedAddressLine1 { get; set; }
        public string? SavedAddressLine2 { get; set; }
        public string? SavedAddressLine3 { get; set; }
        public AUState? SavedState  { get; set; }
        public string? SavedZipCode { get; set; }
        public string? SavedMobileNo { get; set; }
        // Navigation
        public List<Order> Orders { get; set; } = new();
    }
}