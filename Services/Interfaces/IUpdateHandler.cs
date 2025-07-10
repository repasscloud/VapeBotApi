using Telegram.Bot.Types;

namespace VapeBotApi.Services.Interfaces
{
    public interface IUpdateHandler
    {
        Task HandleAsync(Update update);
    }
}
