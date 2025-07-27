using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Stripe;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VapeBotApi.Data;
using VapeBotApi.Models.Admin;
using VapeBotApi.Services;
using VapeBotApi.Services.Interfaces;
using VapeBotApi.Settings;

namespace VapeBotApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        // ── Stripe configuration ───────────────────────────────────────────────────
        // read secret from config (e.g. appsettings.json under "Stripe": { "SecretKey": "..." })
        var stripeSecret = builder.Configuration["Stripe:SecretKey"]
                           ?? throw new InvalidOperationException("Missing Stripe SecretKey!");
        // assign before you ever hit any Stripe client call
        StripeConfiguration.ApiKey = stripeSecret;
        // ───────────────────────────────────────────────────────────────────────────

        // Register DbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        var pgConn = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Missing DefaultConnection!");

        // Pull the bot token
        var botToken = builder.Configuration["Telegram:BotToken"]
                     ?? throw new InvalidOperationException("Missing Telegram bot token.");

        // configure Serilog to PostgreSQL
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.PostgreSQL(
                connectionString: pgConn,
                tableName: "logs",
                columnOptions: ColumnOptions.Default,
                needAutoCreateTable: false
            )
            .CreateLogger();
        builder.Host.UseSerilog();

        // NowPaymentsIO
        builder.Services.Configure<NowPaymentsSettings>(
            builder.Configuration.GetSection("NowPaymentsIO"));

        builder.Services.AddHttpClient("NowPayments", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<NowPaymentsSettings>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
        });

        // PayPal
        builder.Services.Configure<PayPalSettings>(
            builder.Configuration.GetSection("PayPal"));

        builder.Services.AddHttpClient("PayPal", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<PayPalSettings>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
        });

        // Telegram Bot client
        builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));

        // Services
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IUpdateHandler, UpdateDispatcher>();
        builder.Services.AddScoped<IBotMessageStore, BotMessageStore>();
        builder.Services.Configure<PriceCalculatorOptions>(
            builder.Configuration.GetSection("PriceCalculator")
        );
        builder.Services.AddScoped<IPriceCalculatorService, PriceCalculatorService>();

        // MVC Controllers
        builder.Services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                opts.JsonSerializerOptions.WriteIndented     = true;
            });

        // Razor Pages + runtime compilation
        builder.Services
            .AddRazorPages()
            .AddRazorRuntimeCompilation();  // remove if you don't need live-reload

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "API",
                Version     = "v1",
                Description = "Api endpoints"
            });
        });

        var app = builder.Build();

        // Serve static files from wwwroot
        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VapeBot API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseAuthorization();

        // Map MVC controllers and Razor Pages
        app.MapControllers();
        app.MapRazorPages();

        // Telegram webhook & commands
        var telegram = app.Services.GetRequiredService<ITelegramBotClient>();
        await telegram.SetWebhook(
            url: $"https://{builder.Configuration["Ngrok:Host"]}/api/bot/update",
            allowedUpdates: Array.Empty<UpdateType>()
        );
        await telegram.SetMyCommands(new[]
        {
            new BotCommand("create_order",    "Create a new order"),
            new BotCommand("cancel_order",    "Cancel an order"),
            new BotCommand("refund_order",    "Refund an order"),
            new BotCommand("list_to_pack",    "Get orders to pack"),
            new BotCommand("update_shipment", "Update shipment status")
        });
        await telegram.SetChatMenuButton(
            chatId: null,
            menuButton: new MenuButtonCommands()
        );

        var culture = new CultureInfo("en-AU");
        CultureInfo.DefaultThreadCurrentCulture    = culture;
        CultureInfo.DefaultThreadCurrentUICulture  = culture;

        try
        {
            // 4) run the app
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "VapeBot API terminated unexpectedly");
            throw;
        }
        finally
        {
            // 5) flush & close on shutdown
            Log.CloseAndFlush();
        }
    }
}
