using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerRegistryBuilder
{
    IControllerRegistryBuilder RegisterControllerFactoryMethod<TUserState>(
        Func<IControllerContext, AbstractController> factoryMethod)
        where TUserState : UserState;
    
    IControllerFactory Factory { get; }
}
