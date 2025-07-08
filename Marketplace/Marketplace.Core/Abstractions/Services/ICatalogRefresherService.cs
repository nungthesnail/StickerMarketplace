using Marketplace.Core.Abstractions.Data;
using Marketplace.Utils;

namespace Marketplace.Core.Abstractions.Services;

public interface ICatalogRefresherService
{
    event AsyncEventHandler? OnCatalogRefreshed;
    Task RefreshCatalogAsync(CancellationToken stoppingToken = default);
}
