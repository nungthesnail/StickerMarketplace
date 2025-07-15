using System.Text.Json.Serialization;

namespace Marketplace.Bot.Models;

[JsonDerivedType(typeof(InlineKeyboardMarkup), typeDiscriminator: "InlineKeyboardMarkup")]
public abstract class ReplyMarkup;
