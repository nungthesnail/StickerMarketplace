namespace Marketplace.Core.Bot.Models;

public class InlineKeyboardMarkup : ReplyMarkup
{
    public required IEnumerable<IEnumerable<InlineKeyboardButton>> InlineKeyboard { get; set; }
}