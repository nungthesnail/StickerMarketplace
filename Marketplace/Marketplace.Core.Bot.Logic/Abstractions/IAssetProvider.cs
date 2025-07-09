using Marketplace.Core.Bot.Models;

namespace Marketplace.Core.Bot.Logic.Abstractions;

public interface IAssetProvider
{
    string GetTextReplica(string key, out ParseMode parseMode);
    string GetTextReplica(string key) => GetTextReplica(key, out _);
}
