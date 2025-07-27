using Microsoft.EntityFrameworkCore;
using Stripe;
using VapeBotApi.Models;
using VapeBotApi.Models.Admin;
using VapeBotApi.Models.NowPaymentsIO;

namespace VapeBotApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Models.Product> Products { get; set; }
        public DbSet<FaqItem> FaqItems { get; set; }
        public DbSet<ShippingQuote> ShippingQuotes { get; set; }

        public DbSet<DeletedAccount> DeletedAccounts { get; set; }  // used when someone requests an account deletion

        // Make sure this matches your model and uses PascalCase
        public DbSet<BotMessageRecord> BotMessageRecords { get; set; }
        public DbSet<CustomerMessage> CustomerMessages { get; set; }

        public DbSet<PaymentCurrencyInfo> PaymentCurrencyInfos { get; set; }
        public DbSet<CryptoPaymentIntent> CryptoPaymentIntents { get; set; }
    }
}
