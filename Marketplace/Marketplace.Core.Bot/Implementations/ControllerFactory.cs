using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;

namespace Marketplace.Core.Bot.Implementations;

internal sealed class ControllerFactory(
    IReadOnlyDictionary<Type, Func<IControllerCreationContext, IController>> factories)
    : IControllerFactory
{
    public IController? CreateController(IControllerCreationContext creationContext)
    {
        var type = creationContext.UserState.GetType();
        var factory = factories.GetValueOrDefault(type);
        return factory?.Invoke(creationContext);
    }
}
