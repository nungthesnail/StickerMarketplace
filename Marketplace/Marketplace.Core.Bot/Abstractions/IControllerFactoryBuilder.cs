using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerFactoryBuilder
{
    void RegisterControllerFactoryMethod<TController, TUserState>(
        Func<ControllerCreationContext<TUserState>, TController> factoryMethod)
        where TController : AbstractController<TUserState>
        where TUserState : UserState;
    
    IControllerFactory Factory { get; }
}
