using Marketplace.Bot.Models;
using Models_ReplyMarkup = Marketplace.Bot.Models.ReplyMarkup;
using ReplyMarkup = Marketplace.Bot.Models.ReplyMarkup;

namespace Marketplace.Bot.Abstractions;

public interface IBotClient
{
    Task<Message> SendMessageAsync(
        long chatId,
        string text,
        ParseMode parseMode = default,
        Models_ReplyMarkup? replyMarkup = null,
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
        Models_ReplyMarkup? replyMarkup = null,
        bool hasSpoiler = false,
        bool protectContent = false,
        string? messageEffectId = null,
        CancellationToken stoppingToken = default);

    Task<Message> SendAsync(
        long chatId,
        string? fileId = null,
        Stream? photoStream = null,
        string? photoUrl = null,
        string? text = null,
        ParseMode parseMode = default,
        Models_ReplyMarkup? replyMarkup = null,
        bool hasSpoiler = false,
        bool protectContent = false,
        string? messageEffectId = null,
        CancellationToken stoppingToken = default)
    {
        if (photoStream is not null || photoUrl is not null || fileId is not null)
            return SendPhotoAsync(
                chatId,
                fileId,
                photoStream,
                photoUrl,
                text,
                parseMode,
                replyMarkup,
                hasSpoiler,
                protectContent,
                messageEffectId,
                stoppingToken);
        if (text is not null)
            return SendMessageAsync(
                chatId,
                text,
                parseMode,
                replyMarkup,
                protectContent,
                messageEffectId,
                showLinkPreview: false,
                stoppingToken);
        throw new ArgumentNullException(nameof(text));
    }
    
    Task AnswerCallbackQueryAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        string? url = null,
        int? cacheTime = 0,
        CancellationToken stoppingToken = default);
    
    Task<Message> EditMessageTextAsync(long chatId,
        int messageId,
        string text,
        ParseMode parseMode = default,
        Models_ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    Task<Message> EditReplyMarkupAsync(
        long chatId,
        int messageId,
        Models_ReplyMarkup replyMarkup,
        CancellationToken stoppingToken = default);
    
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
    
    Task<Message> SendInvoiceAsync(
        long chatId,
        string title,
        string description,
        string payload,
        string currency,
        IEnumerable<LabeledPrice> prices,
        string? providerToken = null,
        string? providerData = null,
        string? photoUrl = null,
        Models_ReplyMarkup? replyMarkup = null,
        bool protectContent = false,
        string? messageEffectId = null,
        CancellationToken stoppingToken = default);
    
    Task AnswerPreCheckoutQueryAsync(
        string preCheckoutQueryId,
        string? errorMessage = null,
        CancellationToken stoppingToken = default);
    
    Task<(int Amount, int? NanoAmount)> GetMyStarBalanceAsync(CancellationToken stoppingToken = default);
    
    Task RefundStarPaymentAsync(
        long userId,
        string telegramPaymentChargeId,
        CancellationToken stoppingToken = default);
}
