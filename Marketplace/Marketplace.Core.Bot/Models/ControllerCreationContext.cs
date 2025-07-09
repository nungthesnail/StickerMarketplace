using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Models;

public interface IControllerCreationContext<out TUserState>
    where TUserState : UserState
{
    User User { get; }
    TUserState UserState { get; }
    Update Update { get; }
}

public record ControllerCreationContext<TUserState>(User User, TUserState UserState, Update Update)
    : IControllerCreationContext<TUserState>
    where TUserState : UserState;
