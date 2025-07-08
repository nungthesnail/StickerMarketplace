using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public abstract class AbstractController;

public abstract class AbstractController<TUserState>(User user, TUserState userState, Update update)
    : AbstractController
    where TUserState : UserState
{
    protected TUserState UserState => userState;
    protected User User => user;
    protected Update Update => update;
}
