using Mapster;
using Marketplace.Bot.Abstractions;
using Marketplace.Bot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

using ModelsMessage = Marketplace.Bot.Models.Message;
using ModelsReplyMarkup = Marketplace.Bot.Models.ReplyMarkup;
using TgParseMode = Telegram.Bot.Types.Enums.ParseMode;
using TgReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyMarkup;
using TgInlineKeyboardReplyMarkup = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup;
using TgLabeledPrice = Telegram.Bot.Types.Payments.LabeledPrice;

namespace Marketplace.Bot.Telegram.Implementations;

public class TgBotClient(TelegramBotClient clientImpl) : IBotClient
{
    public async Task<ModelsMessage> SendMessageAsync(long chatId, string text, ParseMode parseMode = default,
        ModelsReplyMarkup? replyMarkup = null, bool protectContent = false, string? messageEffectId = null,
        bool showLinkPreview = false, CancellationToken stoppingToken = default)
    {
        var result = await clientImpl.SendMessage(chatId, text, parseMode.Adapt<TgParseMode>(),
            replyMarkup: ReplyMarkupToTg(replyMarkup),
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            linkPreviewOptions: new LinkPreviewOptions
            {
                IsDisabled = !showLinkPreview
            },
            cancellationToken: stoppingToken);
        return result.Adapt<ModelsMessage>();
    }

    private static TgReplyMarkup? ReplyMarkupToTg(ModelsReplyMarkup? replyMarkup)
    {
        return replyMarkup switch
        {
            InlineKeyboardMarkup inline => inline.Adapt<TgInlineKeyboardReplyMarkup>(),
            _ => null
        };
    }

    public async Task<ModelsMessage> SendPhotoAsync(long chatId, string? fileId = null, Stream? photoStream = null, string? photoUrl = null,
        string? caption = null, ParseMode parseMode = default, ModelsReplyMarkup? replyMarkup = null, bool hasSpoiler = false,
        bool protectContent = false, string? messageEffectId = null, CancellationToken stoppingToken = default)
    {
        
        var result = await clientImpl.SendPhoto(
            chatId: chatId,
            photo: CreateFile(fileId, photoStream, photoUrl),
            caption: caption,
            parseMode: parseMode.Adapt<TgParseMode>(),
            replyMarkup: ReplyMarkupToTg(replyMarkup),
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            hasSpoiler: hasSpoiler,
            cancellationToken: stoppingToken);
        return result.Adapt<ModelsMessage>();
    }

    private static InputFile CreateFile(string? fileId, Stream? photoStream, string? photoUrl)
    {
        if (fileId is not null)
            return new InputFileId(fileId);
        if (photoStream is not null)
            return new InputFileStream(photoStream);
        if (photoUrl is not null)
            return new InputFileUrl(photoUrl);
        throw new InvalidOperationException("No file provided");
    }

    public Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, bool showAlert = false, string? url = null,
        int? cacheTime = 0, CancellationToken stoppingToken = default)
        => clientImpl.AnswerCallbackQuery(callbackQueryId, text, showAlert, url, cacheTime, stoppingToken);

    public async Task<ModelsMessage> EditMessageTextAsync(long chatId, int messageId, string text, ParseMode parseMode = default,
        ModelsReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
    {
        var result = await clientImpl.EditMessageText(
            chatId: chatId,
            messageId: messageId,
            text: text,
            parseMode: parseMode.Adapt<TgParseMode>(),
            replyMarkup: ReplyMarkupToTg(replyMarkup) as TgInlineKeyboardReplyMarkup,
            cancellationToken: cancellationToken);
        return result.Adapt<ModelsMessage>();
    }

    public async Task<ModelsMessage> EditReplyMarkupAsync(long chatId, int messageId, ModelsReplyMarkup replyMarkup,
        CancellationToken stoppingToken = default)
    {
        var result = await clientImpl.EditMessageReplyMarkup(
            chatId: chatId,
            messageId: messageId,
            replyMarkup: ReplyMarkupToTg(replyMarkup) as TgInlineKeyboardReplyMarkup,
            cancellationToken: stoppingToken);
        return result.Adapt<ModelsMessage>();
    }

    public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default)
        => clientImpl.DeleteMessage(chatId, messageId, cancellationToken);

    public Task SendGiftAsync(long chatId, string giftId, string? text = null, ParseMode parseMode = default,
        bool payForUpgrade = false, CancellationToken cancellationToken = default)
        => clientImpl.SendGift(chatId, giftId, text, parseMode.Adapt<TgParseMode>(), payForUpgrade: payForUpgrade,
            cancellationToken: cancellationToken);

    public Task GiftPremiumSubscriptionAsync(long userId, int monthCount, int starCount, string? text = null,
        ParseMode parseMode = default, CancellationToken cancellationToken = default)
        => clientImpl.GiftPremiumSubscription(userId, monthCount, starCount, text, parseMode.Adapt<TgParseMode>(),
            cancellationToken: cancellationToken);

    public async Task<ModelsMessage> SendInvoiceAsync(long chatId, string title, string description, string payload, string currency,
        IEnumerable<LabeledPrice> prices, string? providerToken = null, string? providerData = null, string? photoUrl = null,
        ReplyMarkup? replyMarkup = null, bool protectContent = false, string? messageEffectId = null,
        CancellationToken stoppingToken = default)
    {
        var result = await clientImpl.SendInvoice(
            chatId: chatId,
            title: title,
            description: description,
            payload: payload,
            currency: currency,
            prices: prices.Adapt<IEnumerable<TgLabeledPrice>>(),
            providerToken: providerToken,
            providerData: providerData,
            photoUrl: photoUrl,
            replyMarkup: ReplyMarkupToTg(replyMarkup) as TgInlineKeyboardReplyMarkup,
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            cancellationToken: stoppingToken);
        return result.Adapt<ModelsMessage>();
    }

    public Task AnswerPreCheckoutQueryAsync(string preCheckoutQueryId, string? errorMessage = null,
        CancellationToken stoppingToken = default)
        => clientImpl.AnswerPreCheckoutQuery(preCheckoutQueryId, errorMessage, stoppingToken);

    public async Task<(int Amount, int? NanoAmount)> GetMyStarBalanceAsync(CancellationToken stoppingToken = default)
    {
        var balance = await clientImpl.GetMyStarBalance(stoppingToken);
        return (balance.Amount, balance.NanostarAmount);
    }

    public Task RefundStarPaymentAsync(long userId, string telegramPaymentChargeId, CancellationToken stoppingToken = default)
        => clientImpl.RefundStarPayment(userId, telegramPaymentChargeId, stoppingToken);
}
