using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Exceptions;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Implementations.Services;

public class ProjectService(IUnitOfWork uow) : IProjectService
{
    public async Task<bool> IsNameAvailableAsync(string name, CancellationToken stoppingToken = default)
        => !await uow.ProjectRepository.AnyAsync(x => x.Name == name, stoppingToken);
    
    public async Task<bool> IsUrlUniqueAsync(string? url, CancellationToken stoppingToken = default)
        => !await uow.ProjectRepository.AnyAsync(x => x.ContentUrl == url, stoppingToken);

    public async Task CreateProjectAsync(Project project, CancellationToken stoppingToken = default)
    {
        await uow.BeginTransactionAsync(stoppingToken: stoppingToken);
        
        var nameAvailable = await IsNameAvailableAsync(project.Name, stoppingToken);
        var urlIsUnique = await IsUrlUniqueAsync(project.ContentUrl, stoppingToken);
        if (!nameAvailable)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw new ProjectNameAlreadyExistsException(project.Name);
        }

        if (!urlIsUnique)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw new ProjectUrlAlreadyExistsException(project.ContentUrl);
        }
        
        await uow.ProjectRepository.AddAsync(project, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync(CategoryIdentifier? category = null, long? tagId = null,
        CancellationToken stoppingToken = default)
    {
        return await uow.ProjectRepository.GetByAsync(
            predicate: x => (!category.HasValue || x.Category!.Id == category) && (!tagId.HasValue || x.TagId == tagId),
            joins: [nameof(Project.Category)],
            stoppingToken: stoppingToken);
    }

    public async Task<List<Project>> GetProjectsOrderedByRatingAsync(CancellationToken stoppingToken = default)
    {
        return (await uow.ProjectRepository.GetByAsync(
            predicate: x => x.Visible,
            orderBy: x => -x.CachedRating, // Negative value to sort in descending order
            joins: [nameof(Project.User)],
            stoppingToken: stoppingToken)).ToList();
    }

    public async Task<Project?> GetProjectByIdAsync(long projectId, CancellationToken stoppingToken = default)
        => await uow.ProjectRepository.GetByIdAsync(projectId, stoppingToken);
    
    public async Task<Project?> GetProjectByNameAsync(string name, CancellationToken stoppingToken = default)
        => (await uow.ProjectRepository.GetByAsync(x => x.Name == name, stoppingToken: stoppingToken)).FirstOrDefault();
    
    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(long userId,
        CancellationToken stoppingToken = default)
        => await uow.ProjectRepository.GetByAsync(x => x.UserId == userId, stoppingToken: stoppingToken);
    
    public async Task DisableProjectAsync(long projectId, CancellationToken stoppingToken = default)
        => await uow.ProjectRepository.UpdateByAsync(
            predicate: x => x.Id == projectId,
            propertySelector: x => x.Visible,
            valueSelector: _ => false,
            stoppingToken: stoppingToken);
}
