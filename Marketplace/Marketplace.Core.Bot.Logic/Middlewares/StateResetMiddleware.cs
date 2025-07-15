using Marketplace.Bot.Models;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Logic.Middlewares;

public class StateResetMiddleware(IUserStateService userStateService) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (((update.Message?.Text?.StartsWith(AssetKeys.Commands.Start) ?? false)
             || (update.Message?.Text?.StartsWith(AssetKeys.Commands.MainMenu) ?? false))
            && userState is not null)
        {
            userState = new DefaultUserState
            {
                UserId = userState.UserId,
                LastMessageId = userState.LastMessageId
            };
            userStateService.SetUserState(userState.UserId, userState);
        }
        
        await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
    }
}
