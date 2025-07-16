using Marketplace.Core.Abstractions.Services;
using Marketplace.Workers.Abstractions;
using Marketplace.Workers.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marketplace.Workers.Implementations.Services;

public class SubscriptionRefreshWorker(IServiceProvider services, WorkerSettings settings,
    ILogger<SubscriptionRefreshWorker> logger) : AbstractWorker(settings, logger)
{
    protected override async Task PerformIterationAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogDebug("Updating subscriptions");
            
            await using var scope = services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<ISubscriptionRefreshService>();
            await service.UpdateSubscriptionsAsync(stoppingToken);
            
            logger.LogInformation("Subscriptions updated");
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Something failed while refreshing catalog");
        }
    }
}
