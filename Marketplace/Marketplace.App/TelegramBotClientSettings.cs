namespace Marketplace.App;

public record TelegramBotClientSettings(string Token, string? BaseUrl = null, bool UseTestEnvironment = false);
