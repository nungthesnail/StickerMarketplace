using Marketplace.Core.Abstractions.Services;
using Marketplace.Workers.Abstractions;
using Marketplace.Workers.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marketplace.Workers.Implementations.Services;

public class CatalogRefresherServiceWorker(
    IServiceProvider serviceProvider, WorkerSettings settings, ILogger<CatalogRefresherServiceWorker> logger)
    : AbstractWorker(settings, logger)
{
    protected override async Task PerformIterationAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<ICatalogRefresherService>();
            await service.RefreshCatalogAsync(stoppingToken);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Something failed while refreshing catalog");
        }
    }
}
