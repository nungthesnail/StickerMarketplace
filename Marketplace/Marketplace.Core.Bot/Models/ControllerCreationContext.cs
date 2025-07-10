using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Models;

public interface IControllerCreationContext
{
    User User { get; }
    UserState UserState { get; }
    Update Update { get; }
}

public interface IControllerCreationContext<out TUserState> : IControllerCreationContext
    where TUserState : UserState
{
    TUserState TypedUserState => (TUserState)UserState;
}

public record ControllerCreationContext(User User, UserState UserState, Update Update)
    : IControllerCreationContext;
