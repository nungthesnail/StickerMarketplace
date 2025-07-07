using Marketplace.Core.Abstractions.Data;

namespace Marketplace.Core.Abstractions.Services;

public interface ICatalogRefresherService
{
    event EventHandler? OnCatalogRefreshed;
    Task RefreshCatalogAsync(CancellationToken stoppingToken = default);
}
