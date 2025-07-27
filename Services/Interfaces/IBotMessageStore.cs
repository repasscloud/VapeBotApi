namespace VapeBotApi.Services.Interfaces
{
    public interface IBotMessageStore
    {
        Task AddMessageAsync(long chatId, int msgId);
        Task<int> GetLastMessageIdAsync(long chatId);
        Task<List<int>> GetAllCurrentMessageIdsAsync(long chatId);
        Task DeleteMessageAsync(long chatId, int msgId);
        Task DeleteLastMessageAsync(long chatId);
        Task ClearMessageHistoryByChatIdAsync(long chatId);
    }
}
