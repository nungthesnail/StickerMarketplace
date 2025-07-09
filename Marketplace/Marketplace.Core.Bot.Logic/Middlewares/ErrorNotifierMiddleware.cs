using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public class ErrorNotifierMiddleware(ILogger<ErrorNotifierMiddleware> logger, IBotClient bot,
    IAssetProvider assetProvider) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        try
        {
            await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
        }
        catch (Exception)
        {
            var chatId = update.Message?.Chat.Id;
            if (chatId is not null)
            {
                logger.LogDebug("Notifying user {userId} about error", chatId);
                var replica = assetProvider.GetTextReplica(AssetKeys.Text.UpdateHandlingFault, out var parseMode);
                await bot.SendMessageAsync(
                    chatId: chatId.Value,
                    text: replica,
                    parseMode: parseMode,
                    stoppingToken: stoppingToken);
            }

            throw;
        }
    }
}
