using Marketplace.Core.Models.Catalog;

namespace Marketplace.Core.Abstractions.Services;

public interface ICatalogService
{
    Task<CatalogProjectView> GetProjectByIndex(int index, CatalogFilter? filter = null,
        CancellationToken stoppingToken = default);
}
