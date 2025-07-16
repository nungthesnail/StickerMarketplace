using System.Diagnostics.CodeAnalysis;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Catalog;

namespace Marketplace.InMemoryCache.Services;

public class CatalogService : ICatalogService
{
    private readonly IProjectService _projectService;
    private readonly object _updateLock = new();
    
    private List<Project>? _orderedProjects;
    
    public CatalogService(IProjectService projectService, ICatalogRefreshService catalogRefresher)
    {
        _projectService = projectService;
        catalogRefresher.OnCatalogRefreshed += UpdateCatalogAsync;
    }
    
    [MemberNotNull(nameof(_orderedProjects))]
    private async Task UpdateCatalogAsync(CancellationToken stoppingToken = default)
    {
        var projects = await _projectService.GetProjectsOrderedByRatingAsync(stoppingToken);
        lock (_updateLock)
        {
            _orderedProjects = projects;
        }
    }
    
    public async Task<CatalogProjectView> GetProjectByIndexAsync(int index, int direction, CatalogFilter? filter = null,
        CancellationToken stoppingToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(direction); // The iteration should move, not stand
        
        // Search from the index position
        var projects = await GetProjectsAsync(stoppingToken);
        filter ??= new CatalogFilter();
        Predicate<Project> predicate = x
            => x.CategoryId == filter.Category // Match category
               && (filter.TagIds.Count == 0 || filter.TagIds.Contains(x.TagId)) // Match tags
               && ((!x.Moderated && filter.NotModerated) || !filter.NotModerated); // Match moderation
        
        var count = projects.Count;
        var idxInList = index;
        if (index < 0 || index >= count)
            idxInList = Math.Clamp(index, 0, count - 1);

        for (var i = idxInList; i < count && i >= 0; i += Math.Abs(direction))
        {
            if (!predicate.Invoke(projects[i]))
                continue;
            idxInList = i;
            break;
        }
        if (idxInList >= 0 && idxInList < count)
            return CreateView(idxInList);
        
        // If not found, search from the beginning or end depending on the direction of search.
        for (var i = direction < 0 ? idxInList : 0; i < count && i >= 0; i += Math.Abs(direction))
        {
            if (!predicate.Invoke(projects[i]))
                continue;
            idxInList = i;
            break;
        }
        if (idxInList >= 0 && idxInList < count)
            return CreateView(idxInList);

        // If no project found
        return new CatalogProjectView
        {
            CurrentIndex = 0,
            Project = null
        };

        CatalogProjectView CreateView(int idx)
        {
            return new CatalogProjectView
            {
                CurrentIndex = idx,
                Project = projects[idx]
            };
        }
    }
    
    private async Task<List<Project>> GetProjectsAsync(CancellationToken stoppingToken = default)
    {
        if (_orderedProjects is null)
            await UpdateCatalogAsync(stoppingToken);
        return _orderedProjects;
    }
}
