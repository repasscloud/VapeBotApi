using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Controllers
{
    [ApiController]
    [Route("api/bot/update")]
    public class BotController : ControllerBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUpdateHandler _updateHandler;

        public BotController(ITelegramBotClient botClient, IUpdateHandler updateHandler)
        {
            _botClient = botClient;
            _updateHandler = updateHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            // Optional: verify secret/token header
            await _updateHandler.HandleAsync(update);
            return Ok();
        }
    }
}