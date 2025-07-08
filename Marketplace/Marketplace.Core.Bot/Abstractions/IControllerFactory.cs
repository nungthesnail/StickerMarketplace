using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerFactory
{
    TController CreateControllerForUserState<TUserState, TController>(
        ControllerCreationContext<UserState> creationContext)
        where TUserState : UserState
        where TController : AbstractController<TUserState>
    {
        if (creationContext.UserState is not TUserState typedState)
            throw new InvalidOperationException(
                $"Can't create controller of type {typeof(TController)} for user state {creationContext.UserState}");
        
        return CreateControllerForConcreteUserState<TUserState, TController>(
            new ControllerCreationContext<TUserState>(
                User: creationContext.User,
                UserState: typedState,
                Update: creationContext.Update));
    }
    
    TController CreateControllerForConcreteUserState<TUserState, TController>(
        ControllerCreationContext<TUserState> creationContext)
        where TUserState : UserState
        where TController : AbstractController<TUserState>;
}
