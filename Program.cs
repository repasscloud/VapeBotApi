using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using VapeBotApi.Data;
using VapeBotApi.Repositories.Interfaces;
using VapeBotApi.Repositories;
using VapeBotApi.Services.Interfaces;
using VapeBotApi.Services;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System.Text.Json.Serialization;
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

        // Register DbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        var pgConn = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("Missing DefaultConnection!");


        // Pull the bot token and ensure it’s present
        var botToken = builder.Configuration["Telegram:BotToken"]
                    ?? throw new InvalidOperationException("Missing Telegram bot token in configuration.");

        // plug in Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.PostgreSQL(connectionString: pgConn,
            tableName: "logs",
            columnOptions: ColumnOptions.Default,
            needAutoCreateTable: false)
            .CreateLogger();

        // 5) Replace ASP.NET Core’s default logging with Serilog
        builder.Host.UseSerilog();

        // Register Telegram Bot client
        builder.Services.AddSingleton<ITelegramBotClient>(sp =>
            new TelegramBotClient(botToken));

        // Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Services
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IUpdateHandler, UpdateDispatcher>();

        // bind "PriceCalculator" section int othe options POCO
        builder.Services.Configure<PriceCalculatorOptions>(
            builder.Configuration.GetSection("PriceCalculator")
        );
        builder.Services.AddScoped<IPriceCalculatorService, PriceCalculatorService>();

        // Add controllers
        builder.Services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                // serialize each object once, then drop back-references
                opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                // make output easier to read
                opts.JsonSerializerOptions.WriteIndented = true;
            });

        // Swagger setup
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "VapeBot API",
                Version = "v1",
                Description = "Your VapeBot endpoints"
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VapeBot API v1");
                c.RoutePrefix = "swagger"; // correct prefix without leading slash
            });
        }

        app.UseAuthorization();
        app.MapControllers();

        // 1) Set your Telegram webhook
        var telegram = app.Services.GetRequiredService<ITelegramBotClient>();
        await telegram.SetWebhook(
            url: $"https://{builder.Configuration["Ngrok:Host"]}/api/bot/update",
            allowedUpdates: Array.Empty<UpdateType>()
        );

        // 2) Register your bot commands
        await telegram.SetMyCommands(new[]
        {
            new BotCommand("create_order",    "Create a new order"),
            new BotCommand("cancel_order",    "Cancel an order"),
            new BotCommand("refund_order",    "Refund an order"),
            new BotCommand("list_to_pack",    "Get orders to pack"),
            new BotCommand("update_shipment", "Update shipment status")
        });

        // 3) Show the persistent “Menu” button
        await telegram.SetChatMenuButton(
            chatId: null,
            menuButton: new MenuButtonCommands()
        );

        app.Run();
    }
}
