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
    
    public CatalogService(IProjectService projectService, ICatalogRefresherService catalogRefresher)
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
    
    public async Task<CatalogProjectView> GetProjectByIndexAsync(int index, CatalogFilter? filter = null,
        CancellationToken stoppingToken = default)
    {
        // Search from the index position
        var projects = await GetProjectsAsync(stoppingToken);
        filter ??= new CatalogFilter();
        Predicate<Project> predicate = x
            => x.CategoryId == filter.Category
               && (filter.TagIds.Count == 0 || filter.TagIds.Contains(x.TagId));
        
        var count = projects.Count;
        var idxInList = index;
        if (index < 0 || index >= count)
            idxInList = Math.Clamp(index, 0, count - 1);
        
        idxInList = projects.FindIndex(
            startIndex: idxInList,
            match: predicate);
        if (idxInList >= 0)
            return CreateView(idxInList);
        
        // If not found, search from the beginning
        idxInList = projects.FindIndex(
            startIndex: 0,
            match: predicate);
        if (idxInList >= 0)
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
