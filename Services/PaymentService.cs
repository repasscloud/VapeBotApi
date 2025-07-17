using VapeBotApi.Data;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;

        public PaymentService(AppDbContext db)
        {
            _db = db;
        }

        public Task<string> GetStripePaymentLink(long chatId)
        {
            throw new NotImplementedException();
        }
    }
}
