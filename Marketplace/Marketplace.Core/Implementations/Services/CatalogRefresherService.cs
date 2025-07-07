using System.Data;
using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;

namespace Marketplace.Core.Implementations.Services;

public class CatalogRefresherService(IUnitOfWork uow) : ICatalogRefresherService
{
    public event EventHandler? OnCatalogRefreshed;

    public virtual async Task RefreshCatalogAsync(CancellationToken stoppingToken = default)
    {
        await uow.BeginTransactionAsync(isolationLevel: IsolationLevel.Serializable, stoppingToken: stoppingToken);

        try
        {
            var projectMetrics = await GetMetricsForProjectsAsync(stoppingToken);
            await UpdateProjectCachedMetrics(projectMetrics, stoppingToken);
        }
        catch (Exception)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw;
        }

        await uow.CommitTransactionAsync(stoppingToken);
        FireRefreshedNotification();
    }

    // Calculation formula: Likes * Likes / Dislikes.
    protected async Task<IEnumerable<ProjectMetrics>> GetMetricsForProjectsAsync(CancellationToken stoppingToken)
    {
        return await uow.LikeRepository.GetByGroupsAsync(
            predicate: _ => true,
            keySelector: x => x.ProjectId,
            groupSelector: g => new ProjectMetrics(
                g.Key,
                g.LongCount(x => !x.Liked) == 0 // To avoid division by zero
                    ? (double)g.LongCount(x => x.Liked) * g.LongCount(x => x.Liked)
                    : (double)g.LongCount(x => x.Liked) * g.LongCount(x => x.Liked) / g.LongCount(x => !x.Liked)),
            stoppingToken: stoppingToken);
    }
    
    protected async Task UpdateProjectCachedMetrics(IEnumerable<ProjectMetrics> metrics,
        CancellationToken stoppingToken)
    {
        var metricsList = metrics.ToList();
        
        foreach (var projectMetrics in metricsList)
        {
            await uow.ProjectRepository.UpdateByAsync(
                predicate: x => x.Id == projectMetrics.ProjectId,
                propertySelector: x => x.CachedRating,
                valueSelector: _ => projectMetrics.Rating,
                stoppingToken: stoppingToken);
        }

        var presentedIds = metricsList.Select(static x => x.ProjectId).ToList();
        await uow.ProjectRepository.UpdateByAsync(
            predicate: x => !presentedIds.Contains(x.Id),
            propertySelector: x => x.CachedRating,
            valueSelector: _ => 0,
            stoppingToken: stoppingToken);
    }
    
    protected void FireRefreshedNotification()
        => OnCatalogRefreshed?.Invoke(this, EventArgs.Empty);
}
