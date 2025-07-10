using Telegram.Bot.Types;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class UpdateDispatcher : IUpdateHandler
    {
        private readonly IOrderService _orders;
        // inject any other handlers or services (FAQ, Contact, etc.) here

        public UpdateDispatcher(IOrderService orders)
        {
            _orders = orders;
        }

        public async Task HandleAsync(Update update)
        {
            if (update.Message != null)
            {
                // handle text commands and menu
            }
            else if (update.CallbackQuery != null)
            {
                // handle inline button callbacks
            }
            // other update types...
        }
    }
}