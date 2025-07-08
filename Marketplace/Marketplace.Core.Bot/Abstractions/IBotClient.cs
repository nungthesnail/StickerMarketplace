using Marketplace.Core.Bot.Models;
using ReplyMarkup = Marketplace.Core.Bot.Models.ReplyMarkup;

namespace Marketplace.Core.Bot.Abstractions;

public interface IBotClient
{
    Task<Message> SendMessageAsync(
        long chatId,
        string text,
        ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null,
        bool protectContent = false,
        string? messageEffectId = null,
        bool showLinkPreview = false,
        CancellationToken stoppingToken = default);

    Task<Message> SendPhotoAsync(
        long chatId,
        string? fileId = null,
        Stream? photoStream = null,
        string? photoUrl = null,
        string? caption = null,
        ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null,
        bool hasSpoiler = false,
        bool protectContent = false,
        string? messageEffectId = null,
        CancellationToken stoppingToken = default);
    
    Task AnswerCallbackQueryAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        string? url = null,
        int? cacheTime = 0,
        CancellationToken stoppingToken = default);
    
    Task<Message> EditMessageTextAsync(
        long chatId,
        int messageId,
        string text,
        ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null,
        bool showLinkPreview = false,
        CancellationToken cancellationToken = default);
    
    Task DeleteMessageAsync(
        long chatId,
        int messageId,
        CancellationToken cancellationToken = default);
    
    Task SendGiftAsync(
        long chatId,
        string giftId,
        string? text = null,
        ParseMode parseMode = default,
        bool payForUpgrade = false,
        CancellationToken cancellationToken = default);
    
    Task GiftPremiumSubscriptionAsync(
        long userId,
        int monthCount,
        int starCount,
        string? text = null,
        ParseMode parseMode = default,
        CancellationToken cancellationToken = default);
    
    Task SendInvoiceAsync(
        long chatId,
        string title,
        string description,
        string payload,
        string currency,
        IEnumerable<LabeledPrice> prices,
        string? providerToken = null,
        string? providerData = null,
        string? photoUrl = null,
        ReplyMarkup? replyMarkup = null,
        bool protectContent = false,
        string? messageEffectId = null,
        CancellationToken stoppingToken = default);
    
    Task AnswerPreCheckoutQueryAsync(
        string preCheckoutQueryId,
        string? errorMessage = null,
        CancellationToken stoppingToken = default);
    
    Task GetMyStarBalanceAsync(out int amount, out int? nanoAmount, CancellationToken stoppingToken = default);
    
    Task RefundStarPaymentAsync(
        long userId,
        string telegramPaymentChargeId,
        CancellationToken stoppingToken = default);
}
