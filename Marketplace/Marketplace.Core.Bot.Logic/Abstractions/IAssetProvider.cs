using Marketplace.Bot.Models;

namespace Marketplace.Core.Bot.Logic.Abstractions;

public interface IAssetProvider
{
    string GetTextReplica(string key, out ParseMode parseMode, out string? imageUrl, out ReplyMarkup? replyMarkup,
        out string? effectId, params object?[] args);
    string GetTextReplica(string key, out ParseMode parseMode, out string? imageUrl, out ReplyMarkup? replyMarkup,
        params object?[] args) => GetTextReplica(key, out parseMode, out imageUrl, out replyMarkup, out _, args);
    string GetTextReplica(string key, out ParseMode parseMode, out string? imageUrl, params object?[] args)
        => GetTextReplica(key, out parseMode, out imageUrl, out _, args);
    string GetTextReplica(string key, out ParseMode parseMode, params object?[] args)
        => GetTextReplica(key, out parseMode, out _, args);
    string GetTextReplica(string key, params object?[] args) => GetTextReplica(key, out _, args);

    void GetInvoice(string key, out string title, out string description, out string? imageUrl,
        params object?[] titleArgs);
}
