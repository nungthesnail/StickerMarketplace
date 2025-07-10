using System.Collections.ObjectModel;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Implementations;

public class ControllerRegistryBuilder : IControllerRegistryBuilder
{
    private readonly Dictionary<Type, Func<IControllerContext, AbstractController>> _factories = [];
    
    public IControllerRegistryBuilder RegisterControllerFactoryMethod<TUserState>(
        Func<IControllerContext, AbstractController> factoryMethod)
        where TUserState : UserState
    {
        if (!_factories.TryAdd(typeof(TUserState), factoryMethod))
            throw new InvalidOperationException($"Controller for state type {typeof(TUserState)} has already been registered.");
        return this;
    }

    public IControllerFactory Factory => new ControllerFactory(
        new ReadOnlyDictionary<Type, Func<IControllerContext, AbstractController>>(_factories));
}
