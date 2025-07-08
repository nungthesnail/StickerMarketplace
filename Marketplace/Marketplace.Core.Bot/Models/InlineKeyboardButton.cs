namespace Marketplace.Core.Bot.Models;

public class InlineKeyboardButton
{
    public required string Text { get; set; }
    public string? Url { get; set; }
    public string? CallbackData { get; set; }
    public bool Pay { get; set; }
}
