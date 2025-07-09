using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Implementations;

internal sealed class ControllerFactory(
    IReadOnlyDictionary<Type, Func<IControllerCreationContext<UserState>, IController>> factories)
    : IControllerFactory
{
    public IController CreateControllerForConcreteUserState<TUserState>(
        IControllerCreationContext<TUserState> creationContext)
        where TUserState : UserState
    {
        var factory = factories.GetValueOrDefault(typeof(TUserState));
        if (factory is null)
            throw new InvalidOperationException($"No factory registered for controller of state type {typeof(TUserState)}");
        return factory.Invoke(creationContext);
    }
}
