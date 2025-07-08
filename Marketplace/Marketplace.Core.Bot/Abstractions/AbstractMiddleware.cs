using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public abstract class AbstractMiddleware(AbstractMiddleware? next)
{
    public abstract Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default);

    protected async Task InvokeNextAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken)
    {
        if (next is null)
            return;
        await next.InvokeAsync(user, userState, update, stoppingToken);
    }
}
