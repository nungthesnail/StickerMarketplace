using Marketplace.Core.Bot.Models;

namespace Marketplace.Core.Bot.Abstractions;

public interface IControllerFactory
{
    AbstractController? CreateController(IControllerContext context);
}
