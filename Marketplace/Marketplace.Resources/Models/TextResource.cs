using Marketplace.Bot.Models;

namespace Marketplace.Resources.Models;

public record TextResource(string Text, ParseMode ParseMode = default, string? ImageUrl = null,
    ReplyMarkup? ReplyMarkup = null, string? EffectId = null);
