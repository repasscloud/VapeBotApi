using Telegram.Bot;
using Telegram.Bot.Types;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class UpdateDispatcher : IUpdateHandler
    {
        private readonly ILogger<UpdateDispatcher> _log;
        private readonly ITelegramBotClient _botClient;
        private readonly IOrderService _orderService;
        // inject any other handlers or services (FAQ, Contact, etc.) here

        public UpdateDispatcher(
            ILogger<UpdateDispatcher> log,
            ITelegramBotClient botClient,
            IOrderService     orderService)
        {
            _log          = log;
            _botClient    = botClient;
            _orderService = orderService;
        }

        public async Task HandleAsync(Update update)
        {
            // menu button command
            if (update.Message?.Text is { } text && text.StartsWith("/"))
            {
                // a slash-command came in
                switch (text.Split(' ')[0])
                {
                    case "/create_order":
                        _log.LogDebug($"msg_txt: /create_order");
                        break;
                }
                return;
            }

            // button presses (inline-keyboard buttons) -> CallbackQuery
            if (update.CallbackQuery is { } cq)
            {
                var data   = cq.Data;
                var chatId = cq.Message?.Chat.Id;
                return;
            }

            // reply-keyboard buttons -> just plain text
            // update.Message.Text equals the button's label
            if (update.Message?.Text is { } replyText)
            {
                // if you sent a custom keyboard with row like ["Yes", "No"],
                // pressing "Yes" yields update.Message.Text == "Yes"
                // handle it to distinguish from free-form text
                return;
            }

            // content message (non-text)
            if (update.Message?.Photo?.Length > 0 is true)
            {
                // user sent one or more photos
                var fileId = update.Message.Photo.Last().FileId;
                // download or process
                return;
            }

            if (update.Message?.Voice is { } voice)
            {
                // user sent a voice note
                var voiceFileId = voice.FileId;
                // download or process
                return;
            }

            // (can also handle Video, Document, Location, Contact, etc. in the same way:
            // check update.Message.Video, update.Message.Document, update.Message.Location, etc.)

            // --- Fallback: unhandled update ---
            // 1) Log it
            Console.WriteLine($"[❓] Unhandled update type: {update.Type}");
            // or, if you have injected ILogger<UpdateDispatcher> _logger:
            // _logger.LogWarning("Unhandled update of type {UpdateType}: {@Update}", update.Type, update);

            // 2) Optionally notify the user (if it makes sense)
            if (update.Message?.Chat != null)
            {
                await _botClient.SendMessage(
                    chatId: update.Message.Chat.Id,
                    text: "Sorry, I didn't understand that. Please use the menu or /commands."
                );
            }
        }
    }
}
