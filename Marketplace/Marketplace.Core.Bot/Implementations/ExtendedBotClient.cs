using Marketplace.Bot.Abstractions;
using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Implementations;

public class ExtendedBotClient(IBotClient botClient, ILogger<ExtendedBotClient> logger) : IExtendedBotClient
{
    public Task<Message> SendMessageAsync(long chatId, string text, ParseMode parseMode = default, ReplyMarkup? replyMarkup = null,
        bool protectContent = false, string? messageEffectId = null, bool showLinkPreview = false,
        CancellationToken stoppingToken = default)
        => botClient.SendMessageAsync(chatId, text, parseMode, replyMarkup, protectContent, messageEffectId, showLinkPreview,
            stoppingToken);

    public Task<Message> SendPhotoAsync(long chatId, string? fileId = null, Stream? photoStream = null,
        string? photoUrl = null,
        string? caption = null, ParseMode parseMode = default, ReplyMarkup? replyMarkup = null, bool hasSpoiler = false,
        bool protectContent = false, string? messageEffectId = null, CancellationToken stoppingToken = default)
        => botClient.SendPhotoAsync(chatId, fileId, photoStream, photoUrl, caption, parseMode, replyMarkup, hasSpoiler,
            protectContent, messageEffectId, stoppingToken);

    public Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, bool showAlert = false, string? url = null,
        int? cacheTime = 0, CancellationToken stoppingToken = default)
        => botClient.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert, url, cacheTime, stoppingToken);

    public Task<Message> EditMessageTextAsync(long chatId, int messageId, string text, ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
        => botClient.EditMessageTextAsync(chatId, messageId, text, parseMode, replyMarkup, cancellationToken);

    public Task<Message> EditReplyMarkupAsync(long chatId, int messageId, ReplyMarkup replyMarkup,
        CancellationToken stoppingToken = default)
        => botClient.EditReplyMarkupAsync(chatId, messageId, replyMarkup, stoppingToken);

    public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default)
        => botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);

    public Task SendGiftAsync(long chatId, string giftId, string? text = null, ParseMode parseMode = default,
        bool payForUpgrade = false, CancellationToken cancellationToken = default)
        => botClient.SendGiftAsync(chatId, giftId, text, parseMode, payForUpgrade, cancellationToken);

    public Task GiftPremiumSubscriptionAsync(long userId, int monthCount, int starCount, string? text = null,
        ParseMode parseMode = default, CancellationToken cancellationToken = default)
        => botClient.GiftPremiumSubscriptionAsync(userId, monthCount, starCount, text, parseMode, cancellationToken);

    public Task<Message> SendInvoiceAsync(long chatId, string title, string description, string payload,
        string currency,
        IEnumerable<LabeledPrice> prices, string? providerToken = null, string? providerData = null,
        string? photoUrl = null,
        ReplyMarkup? replyMarkup = null, bool protectContent = false, string? messageEffectId = null,
        CancellationToken stoppingToken = default)
        => botClient.SendInvoiceAsync(chatId, title, description, payload, currency, prices, providerToken, providerData,
            photoUrl, replyMarkup, protectContent, messageEffectId, stoppingToken);

    public Task AnswerPreCheckoutQueryAsync(string preCheckoutQueryId, string? errorMessage = null,
        CancellationToken stoppingToken = default)
        => botClient.AnswerPreCheckoutQueryAsync(preCheckoutQueryId, errorMessage, stoppingToken);

    public Task<(int Amount, int? NanoAmount)> GetMyStarBalanceAsync(CancellationToken stoppingToken = default)
        => botClient.GetMyStarBalanceAsync(stoppingToken);

    public Task RefundStarPaymentAsync(long userId, string telegramPaymentChargeId, CancellationToken stoppingToken = default)
        => botClient.RefundStarPaymentAsync(userId, telegramPaymentChargeId, stoppingToken);

    public async Task<Message> SendMessageAndDeleteLastAsync(UserState userState, string text, ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null, bool protectContent = false, string? messageEffectId = null, bool showLinkPreview = false,
        CancellationToken stoppingToken = default)
    {
        await TryDeleteLastMessageAsync(userState, stoppingToken);
        return await SendMessageAsync(userState.UserId, text, parseMode, replyMarkup, protectContent, messageEffectId,
            showLinkPreview, stoppingToken);
    }

    private async Task TryDeleteLastMessageAsync(UserState userState, CancellationToken stoppingToken)
    {
        if (userState.LastMessageId is not null)
        {
            try
            {
                await DeleteMessageAsync(userState.UserId, userState.LastMessageId.Value, stoppingToken);
            }
            catch (Exception exc)
            {
                logger.LogTrace(exc, "Can't delete the last user's message");
            }
        }
    }

    public async Task<Message> SendPhotoAndDeleteLastAsync(UserState userState, string? fileId = null, Stream? photoStream = null,
        string? photoUrl = null, string? caption = null, ParseMode parseMode = default, ReplyMarkup? replyMarkup = null,
        bool hasSpoiler = false, bool protectContent = false, string? messageEffectId = null, CancellationToken stoppingToken = default)
    {
        await TryDeleteLastMessageAsync(userState, stoppingToken);
        return await botClient.SendPhotoAsync(userState.UserId, fileId, photoStream, photoUrl, caption, parseMode,
            replyMarkup, hasSpoiler, protectContent, messageEffectId, stoppingToken);
    }

    public async Task<Message> SendAsync(UserState userState, string? fileId = null, Stream? photoStream = null, string? photoUrl = null,
        string? text = null, ParseMode parseMode = default, ReplyMarkup? replyMarkup = null, bool hasSpoiler = false,
        bool protectContent = false, string? messageEffectId = null, CancellationToken stoppingToken = default)
    {
        var photoProvided = fileId is not null || photoStream is not null || photoUrl is not null;
        if (photoProvided)
            return await SendPhotoAndDeleteLastAsync(userState, fileId, photoStream, photoUrl, text, parseMode,
                replyMarkup, hasSpoiler, protectContent, messageEffectId, stoppingToken);
        if (text is not null)
            return await SendMessageAndDeleteLastAsync(userState, text, parseMode, replyMarkup, protectContent,
                messageEffectId, false, stoppingToken);
        throw new InvalidOperationException($"Can't send message to user. Text is null: {text}, photo is provided: {photoProvided}");
    }

    public Task<Message> EditMessageTextAsync(UserState userState, int messageId, string text, ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null, bool showLinkPreview = false, CancellationToken cancellationToken = default)
        => botClient.EditMessageTextAsync(userState.UserId, messageId, text, parseMode, replyMarkup, cancellationToken);
}
