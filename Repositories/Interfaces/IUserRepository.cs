using VapeBotApi.Models;

namespace VapeBotApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetOrCreateAsync(long chatId);
        Task SaveAsync();
    }
}