using System.Diagnostics.CodeAnalysis;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Catalog;

namespace Marketplace.InMemoryCache.Services;

public class CatalogService
{
    private readonly IProjectService _projectService;
    private readonly object _updateLock = new();
    
    private List<Project>? _orderedProjects;
    private List<Project> Projects
    {
        get
        {
            if (_orderedProjects is null)
                UpdateCatalog();
            return _orderedProjects;
        }
    }

    public CatalogService(IProjectService projectService, ICatalogRefresherService catalogRefresher)
    {
        _projectService = projectService;
        catalogRefresher.OnCatalogRefreshed += (_, _) => UpdateCatalog();
    }
    
    [MemberNotNull(nameof(_orderedProjects))]
    private void UpdateCatalog()
    {
        lock (_updateLock)
        {
            _orderedProjects = _projectService.GetProjectsOrderedByRatingAsync().GetAwaiter().GetResult();
        }
    }
    
    public CatalogProjectView GetProjectByIndex(int index, CatalogFilter? filter = null,
        CancellationToken stoppingToken = default)
    {
        // Search from the index position
        filter ??= new CatalogFilter();
        Predicate<Project> predicate = x
            => x.CategoryId == filter.Category
               && (filter.TagIds.Count == 0 || filter.TagIds.Contains(x.TagId));
        
        var count = Projects.Count;
        var idxInList = index;
        if (index < 0 || index >= count)
            idxInList = Math.Clamp(index, 0, count - 1);
        
        idxInList = Projects.FindIndex(
            startIndex: idxInList,
            match: predicate);
        if (idxInList >= 0)
            return CreateView(idxInList);
        
        // If not found, search from the beginning
        idxInList = Projects.FindIndex(
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
                Project = Projects[idx]
            };
        }
    }
}
