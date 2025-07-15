using Marketplace.Bot.Models;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Bot.Implementations.Middlewares;

public class ControllerDispatcherMiddleware(IControllerFactory controllerFactory) : AbstractMiddleware
{
    public override async Task InvokeAsync(User? user, UserState? userState, Update update,
        CancellationToken stoppingToken = default)
    {
        if (user is not null && userState is not null)
        {
            var ctx = new ControllerContext(user, userState, update);
            var controller = controllerFactory.CreateController(ctx);
            if (controller is not null)
            {
                await controller.HandleUpdateAsync(stoppingToken);
                return;
            }
        }

        await (Next?.InvokeAsync(user, userState, update, stoppingToken) ?? Task.CompletedTask);
    }
}
