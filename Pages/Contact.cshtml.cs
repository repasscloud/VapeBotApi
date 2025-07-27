using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VapeBotApi.Models.Admin;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IAdminService _adminService;

        public ContactModel(IAdminService adminService)
            => _adminService = adminService;

        [BindProperty(SupportsGet = true)]
        public long ChatId { get; set; }

        [BindProperty]
        public CustomerMessage MessageModel { get; set; } = default!;

        public void OnGet()
        {
            MessageModel = new CustomerMessage
            {
                Id = 0,
                ChatId = ChatId,
                Message = string.Empty,
                Created = DateTime.UtcNow,
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var routeResult = await _adminService.CreateContactSupportMsgAsync(MessageModel);
            return Redirect(routeResult);
        }
    }
}
