using System.Collections.ObjectModel;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Implementations;

internal class ControllerFactory(
    ReadOnlyDictionary<Type, Func<IControllerCreationContext<UserState>, AbstractController>> factories)
    : IControllerFactory
{
    public TController CreateControllerForConcreteUserState<TUserState, TController>(
        ControllerCreationContext<TUserState> creationContext)
        where TUserState : UserState
        where TController : AbstractController<TUserState>
    {
        var factory = factories.GetValueOrDefault(typeof(TUserState));
        if (factory is null)
            throw new InvalidOperationException($"No factory registered for controller of state type {typeof(TUserState)}");
        var controller = factory.Invoke(creationContext);
        if (controller is not TController typedController)
            throw new InvalidOperationException($"Controller with not expected type ({controller.GetType()}) are registered for state type {typeof(TUserState)}");
        return typedController;
    }
}
