using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IController
{
    public Task HandleUpdateAsync(CancellationToken stoppingToken = default);
}

public abstract class AbstractController<TUserState>(User user, TUserState userState, Update update)
    : IController
    where TUserState : UserState
{
    protected TUserState UserState => userState;
    protected User User => user;
    protected Update Update => update;
    public abstract Task HandleUpdateAsync(CancellationToken stoppingToken = default);
}
