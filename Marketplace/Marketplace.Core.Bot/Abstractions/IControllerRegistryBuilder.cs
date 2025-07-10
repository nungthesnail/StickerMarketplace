using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerRegistryBuilder
{
    IControllerRegistryBuilder RegisterControllerFactoryMethod<TUserState>(
        Func<IControllerCreationContext, IController> factoryMethod)
        where TUserState : UserState;
    
    IControllerFactory Factory { get; }
}
