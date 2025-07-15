using Marketplace.Core.Models.Catalog;

namespace Marketplace.Core.Abstractions.Services;

public interface ICatalogService
{
    Task<CatalogProjectView> GetProjectByIndexAsync(int index, int direction, CatalogFilter? filter = null,
        CancellationToken stoppingToken = default);
}
