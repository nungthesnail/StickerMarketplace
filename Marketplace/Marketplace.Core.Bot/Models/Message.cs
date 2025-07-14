namespace Marketplace.Core.Bot.Models;

public class Message
{
    public int Id { get; set; }
    public BotUser? From { get; set; }
    public BotChat? SenderChat { get; set; }
    public DateTime Date { get; set; }
    public BotChat Chat { get; set; } = null!;
    public bool HasProtectedContent { get; set; }
    public string? Text { get; set; }
    public string? EffectId { get; set; }
    public string? Caption { get; set; }
    public bool HasMediaSpoiler { get; set; }
    public PhotoSize? Photo { get; set; }
    public SuccessfulPayment? SuccessfulPayment { get; set; }
    public InlineKeyboardMarkup? ReplyMarkup { get; set; }
}
