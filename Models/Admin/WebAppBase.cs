namespace VapeBotApi.Models.Admin
{
    public static class WebAppBase
    {
        public static string Url { get; set; } = "https://secure-endlessly-puma.ngrok-free.app";
        public static string NullOrderIdUrl { get; set; } = "https://secure-endlessly-puma.ngrok-free.app/order-is-null/index.html";
        public static string StripePaymentGenerationErrorUrl { get; set; } = $"{Url}/error/stripe-generate-payment-link.html";
        public static string PaymentSuccessUrl { get; set; } = $"{Url}/payment-success";
        public static string PaymentFailedUrl { get; set; } = $"{Url}/payment-failed";
        public static string NowPaymentsPaymentGenerationErrorUrl { get; set; } = $"{Url}/error/nowpayments-generate-payment-link.html";
    }
}