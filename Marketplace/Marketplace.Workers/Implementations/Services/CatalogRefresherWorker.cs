using Marketplace.Core.Abstractions.Services;
using Marketplace.Workers.Abstractions;
using Marketplace.Workers.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marketplace.Workers.Implementations.Services;

public class CatalogRefresherWorker(
    IServiceProvider serviceProvider, WorkerSettings settings, ILogger<CatalogRefresherWorker> logger)
    : AbstractWorker(settings, logger)
{
    protected override async Task PerformIterationAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogDebug("Refreshing catalog");
            
            await using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<ICatalogRefreshService>();
            await service.RefreshCatalogAsync(stoppingToken);
            
            logger.LogInformation("Catalog refreshed");
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Something failed while refreshing catalog");
        }
    }
}
