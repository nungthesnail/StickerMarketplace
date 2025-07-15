using Marketplace.Bot.Abstractions;
using Marketplace.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IExtendedBotClient : IBotClient
{
    Task<Message> SendMessageAsync(
        UserState userState,
        string text,
        ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null,
        bool protectContent = false,
        string? messageEffectId = null,
        bool showLinkPreview = false,
        CancellationToken stoppingToken = default);
    
    Task<Message> SendPhotoAsync(
        UserState userState,
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

    Task<Message> SendAsync(
        UserState userState,
        string? fileId = null,
        Stream? photoStream = null,
        string? photoUrl = null,
        string? text = null,
        ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null,
        bool hasSpoiler = false,
        bool protectContent = false,
        string? messageEffectId = null,
        CancellationToken stoppingToken = default);
    
    Task<Message> EditMessageTextAsync(
        UserState userState,
        int messageId,
        string text,
        ParseMode parseMode = default,
        ReplyMarkup? replyMarkup = null,
        bool showLinkPreview = false,
        CancellationToken cancellationToken = default);
}
