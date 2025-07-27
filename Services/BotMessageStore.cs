using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using VapeBotApi.Data;
using VapeBotApi.Models.Admin;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class BotMessageStore : IBotMessageStore
    {
        private readonly AppDbContext _db;

        public BotMessageStore(AppDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task AddMessageAsync(long chatId, int msgId)
        {
            var record = new BotMessageRecord
            {
                ChatId = chatId,
                MessageId = msgId,
                IsDeleted = false
            };
            await _db.BotMessageRecords.AddAsync(record);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetLastMessageIdAsync(long chatId)
        {
            return await _db.BotMessageRecords
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Select(m => (int?)m.MessageId)
                .FirstOrDefaultAsync() ?? -1;
        }

        public async Task<List<int>> GetAllCurrentMessageIdsAsync(long chatId)
        {
            var ids = await _db.BotMessageRecords
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .Select(m => m.MessageId)
                .ToListAsync();

            return ids.Count == 0 ? new List<int> { -1 } : ids;
        }

        public async Task DeleteMessageAsync(long chatId, int msgId)
        {
            var record = await _db.BotMessageRecords
                .FirstOrDefaultAsync(m => m.ChatId == chatId && m.MessageId == msgId && !m.IsDeleted);

            if (record is null)
                return;

            record.IsDeleted = true;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteLastMessageAsync(long chatId)
        {
            var record = await _db.BotMessageRecords
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (record is null)
                return;

            record.IsDeleted = true;
            await _db.SaveChangesAsync();
        }

        public async Task ClearMessageHistoryByChatIdAsync(long chatId)
        {
            await _db.BotMessageRecords
                .Where(m => m.ChatId == chatId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.IsDeleted, true)
                );
        }
    }
}
