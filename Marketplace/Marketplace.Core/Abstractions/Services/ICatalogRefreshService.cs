using Marketplace.Core.Abstractions.Data;
using Marketplace.Utils;

namespace Marketplace.Core.Abstractions.Services;

public interface ICatalogRefreshService
{
    event AsyncEventHandler? OnCatalogRefreshed;
    Task RefreshCatalogAsync(CancellationToken stoppingToken = default);
}
