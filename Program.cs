using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using VapeBotApi.Data;
using VapeBotApi.Repositories;
using VapeBotApi.Repositories.Interfaces;
using VapeBotApi.Services;
using VapeBotApi.Services.Interfaces;

namespace VapeBotApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Register DbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Pull the bot token and ensure it’s present
        var botToken = builder.Configuration["Telegram:BotToken"]
                    ?? throw new InvalidOperationException("Missing Telegram bot token in configuration.");

        // Register Telegram Bot client
        builder.Services.AddSingleton<ITelegramBotClient>(sp =>
            new TelegramBotClient(botToken));

        // Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Services
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IUpdateHandler, UpdateDispatcher>();

        // Add services to the container.
        builder.Services.AddControllers();

        // 1) register the Swagger generator and configure a doc
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

        // 2) enable middleware to serve the generated JSON and the UI
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();                // serves /swagger/v1/swagger.json
            app.UseSwaggerUI(c =>            // serves the nice UI at /swagger
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VapeBot API v1");
                c.RoutePrefix = "/swagger";         // serve UI at root ("/swagger"), omit if you want /swagger
            });
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
