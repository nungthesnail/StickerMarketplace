using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerFactory
{
    AbstractController? CreateController(IControllerContext context);
}
