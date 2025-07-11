using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Services;

public interface IProjectTagService
{
    Task<bool> IsNameAvailableAsync(string name, CancellationToken stoppingToken);
    Task CreateTagAsync(ProjectTag tag, CancellationToken stoppingToken);
    Task<IEnumerable<ProjectTag>> GetTagsAsync(CancellationToken stoppingToken);
}