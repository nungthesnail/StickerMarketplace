using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Abstractions.Services;

public interface IProjectService
{
    Task<bool> IsNameAvailableAsync(string name, CancellationToken stoppingToken = default);
    Task<bool> IsUrlUniqueAsync(string? url, CancellationToken stoppingToken = default);
    
    Task CreateProjectAsync(Project project, CancellationToken stoppingToken = default);

    Task<IEnumerable<Project>> GetProjectsAsync(CategoryIdentifier? category = null, long? tagId = null,
        CancellationToken stoppingToken = default);
    Task<List<Project>> GetProjectsOrderedByRatingAsync(CancellationToken stoppingToken = default);
    Task<Project?> GetProjectByIdAsync(long projectId, CancellationToken stoppingToken = default);
    Task<Project?> GetProjectByNameAsync(string name, CancellationToken stoppingToken = default);
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(long userId,
        CancellationToken stoppingToken = default);

    Task DisableProjectAsync(long projectId, CancellationToken stoppingToken = default);
    Task DeleteProjectAsync(long projectId, CancellationToken stoppingToken = default);
}
