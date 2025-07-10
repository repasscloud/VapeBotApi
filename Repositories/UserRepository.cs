using VapeBotApi.Data;
using VapeBotApi.Models;
using VapeBotApi.Repositories.Interfaces;

namespace VapeBotApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User> GetOrCreateAsync(long chatId)
        {
            var user = await _db.Users.FindAsync(chatId);
            if (user == null)
            {
                user = new User { ChatId = chatId };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            return user;
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}