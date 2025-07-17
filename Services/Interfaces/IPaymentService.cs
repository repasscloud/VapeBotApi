namespace VapeBotApi.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> GetStripePaymentLink(long chatId);
    }
}
