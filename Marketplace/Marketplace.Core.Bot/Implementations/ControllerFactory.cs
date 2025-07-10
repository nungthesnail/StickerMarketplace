using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;

namespace Marketplace.Core.Bot.Implementations;

internal sealed class ControllerFactory(
    IReadOnlyDictionary<Type, Func<IControllerContext, AbstractController>> factories)
    : IControllerFactory
{
    public AbstractController? CreateController(IControllerContext context)
    {
        var type = context.UserState.GetType();
        var factory = factories.GetValueOrDefault(type);
        return factory?.Invoke(context);
    }
}
