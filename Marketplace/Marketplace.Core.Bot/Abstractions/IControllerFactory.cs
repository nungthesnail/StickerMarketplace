using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerFactory
{
    IController CreateControllerForUserState<TUserState>(
        IControllerCreationContext<UserState> creationContext)
        where TUserState : UserState
    {
        if (creationContext.UserState is not TUserState typedState)
            throw new InvalidOperationException(
                $"Can't create controller for user state {creationContext.UserState}");
        
        return CreateControllerForConcreteUserState(
            new ControllerCreationContext<TUserState>(
                User: creationContext.User,
                UserState: typedState,
                Update: creationContext.Update));
    }
    
    IController CreateControllerForConcreteUserState<TUserState>(
        IControllerCreationContext<TUserState> creationContext)
        where TUserState : UserState;
}
