namespace Marketplace.Core.Bot.Models;

public class CallbackQuery
{
    public string Id { get; set; } = null!;
    public BotUser From { get; set; } = null!;
    public Message? Message { get; set; }
    public string? InlineMessageId { get; set; }
    public string? Data { get; set; }
}
