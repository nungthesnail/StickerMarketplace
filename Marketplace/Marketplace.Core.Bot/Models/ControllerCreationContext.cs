using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Models;

public interface IControllerContext
{
    User User { get; }
    UserState UserState { get; }
    Update Update { get; }
    public IControllerContext Copy();
    public IControllerContext CopyWithState(UserState userState);
}

public record ControllerContext(User User, UserState UserState, Update Update)
    : IControllerContext
{
    public IControllerContext Copy() => new ControllerContext(User, UserState, Update);

    public IControllerContext CopyWithState(UserState userState)
        => this with { UserState = userState };
}
