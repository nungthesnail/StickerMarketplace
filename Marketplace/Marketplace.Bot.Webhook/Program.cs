using Mapster;
using Marketplace.App;
using Marketplace.Core.Bot.Abstractions;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

using ILogger = Microsoft.Extensions.Logging.ILogger;
using UpdateDto = Marketplace.Bot.Models.Update;
using TgUpdate = Telegram.Bot.Types.Update;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureBusinessServices(config);

ConfigureSerilog();
builder.Logging.AddSerilog();

var app = builder.Build();

var baseUrl = config.GetValue<string>("BotWebhookBaseUrl")
    ?? throw new InvalidOperationException("Can't set bot webhook because base url is not provided");
var webhookEndpoint = config.GetValue<string>("WebhookEndpoint")
    ?? throw new InvalidOperationException("Can't set webhook because webhook endpoint is not provided");
var webhookUrl = $"{baseUrl}{webhookEndpoint}";
await InitBotAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet($"{webhookEndpoint}/setWebhook", async (TelegramBotClient botClient, ILogger logger) =>
{
    await SetBotWebhookAsync(botClient, config, webhookUrl);
    logger.LogInformation("Bot webhook is set to {url}", webhookUrl);
});

app.MapPost(webhookEndpoint, async (ServiceProvider services, TgUpdate update, CancellationToken stoppingToken) =>
{
    var pipelineInvocator = services.GetRequiredService<IUpdatePipelineInvocator>();
    var updateDto = update.Adapt<UpdateDto>();
    await pipelineInvocator.InvokePipelineAsync(updateDto, stoppingToken);
});

app.UseHttpsRedirection();
app.Run();

return;

void ConfigureSerilog()
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .Enrich.FromLogContext()
        .CreateLogger();
}

async Task InitBotAsync()
{
    var bot = app.Services.GetRequiredService<TelegramBotClient>();
    await SetBotWebhookAsync(bot, config, webhookUrl);
    var me = await bot.GetMe();
    Console.WriteLine($"Bot info: Id={me.Id}, Username={me.Username}, Webhook={webhookUrl}");
}

static async Task SetBotWebhookAsync(TelegramBotClient botClient, IConfiguration config, string webhookUrl)
{
    var sslCertPath = config.GetValue<string>("SSLCertificatePath")
                      ?? throw new InvalidOperationException("SSL certificate path is not provided");
    await using var cert = File.OpenRead(sslCertPath);
    await botClient.SetWebhook(webhookUrl, new InputFileStream(cert));
}
