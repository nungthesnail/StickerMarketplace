using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Resources.Models;

namespace Marketplace.Resources.Implementations;

public class AssetProvider(
    IReadOnlyDictionary<string, TextResource> textResources,
    IReadOnlyDictionary<string, InvoiceResource> invoiceResources)
    : IAssetProvider
{
    public string GetTextReplica(string key, out ParseMode parseMode, out string? imageUrl,
        out ReplyMarkup? replyMarkup, out string? effectId, params object?[] args)
    {
        if (!textResources.TryGetValue(key, out var resource))
            throw new ArgumentException("Key not found");
        parseMode = resource.ParseMode;
        imageUrl = resource.ImageUrl;
        replyMarkup = resource.ReplyMarkup;
        effectId = resource.EffectId;
        return string.Format(resource.Text, args);
    }

    public void GetInvoice(string key, out string title, out string description, out string? imageUrl,
        params object?[] titleArgs)
    {
        if (!invoiceResources.TryGetValue(key, out var resource))
            throw new ArgumentException("Key not found");
        title = string.Format(resource.Title, titleArgs);
        description = resource.Description;
        imageUrl = resource.ImageUrl;
    }
}
