using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Implementations.Services;

public class ProjectCategoryService(IUnitOfWork uow) : IProjectCategoryService
{
    public async Task<IEnumerable<ProjectCategory>> GetAllAsync(CancellationToken stoppingToken)
        => await uow.ProjectCategoryRepository.GetAllAsync(stoppingToken);

    public async Task<ProjectCategory?> GetByIdAsync(CategoryIdentifier id, CancellationToken stoppingToken)
        => await uow.ProjectCategoryRepository.GetByIdAsync(id, stoppingToken);
    
    public async Task<bool> CategoryExistsAsync(CategoryIdentifier id, CancellationToken stoppingToken)
        => await uow.ProjectCategoryRepository.AnyAsync(x => x.Id == id, stoppingToken);
    
    public async Task<bool> CategoryExistsAsync(string name, CancellationToken stoppingToken)
        => await uow.ProjectCategoryRepository.AnyAsync(x => x.Name == name, stoppingToken);
    
    public async Task AddAsync(ProjectCategory category, CancellationToken stoppingToken)
    {
        await uow.BeginTransactionAsync(stoppingToken: stoppingToken);
        var idExists = await CategoryExistsAsync(category.Id, stoppingToken);
        var nameExists = await CategoryExistsAsync(category.Name, stoppingToken);
        if (idExists || nameExists)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw new InvalidOperationException("Category already exists");
        }
        await uow.ProjectCategoryRepository.AddAsync(category, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }
}
