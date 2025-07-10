using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerFactory
{
    IController? CreateController(IControllerCreationContext creationContext);
}
