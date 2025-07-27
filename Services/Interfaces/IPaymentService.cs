using VapeBotApi.Models.NowPaymentsIO;

namespace VapeBotApi.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> GetStripePaymentLink(string orderId);
        Task ProcessCheckoutSessionAsync(string paymentIntentId, string orderRef);
        Task<string?> GetPayPalInvoiceAsync(string customerEmail, string description, decimal amount);
        Task<string> GetNowPaymentsIOPaymentLinkAsync(string orderId);
        Task<SupportedCurrenciesResponse?> GetNowPaymentsIOSupportedCurrenciesAsync();
        Task<List<PaymentCurrencyInfo>> GetAllNowPaymentsIOCurrencyInfosAsync();
        Task<PaymentCurrencyInfo?> GetNowPaymentsIOCurrencyInfoAsync(string currencyCodeFull);
        Task AddOrUpdateNowPaymentsIOCurrencyInfoAsync(PaymentCurrencyInfo info);
        Task<bool> DeleteNowPaymentsIOCurrencyInfoAsync(string currencyCodeFull);
    }
}
