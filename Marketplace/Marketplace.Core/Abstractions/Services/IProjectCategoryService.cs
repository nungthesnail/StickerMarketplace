using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Abstractions.Services;

public interface IProjectCategoryService
{
    Task<IEnumerable<ProjectCategory>> GetAllAsync(CancellationToken stoppingToken);
    Task<ProjectCategory?> GetByIdAsync(CategoryIdentifier id, CancellationToken stoppingToken);
    Task<bool> CategoryExistsAsync(CategoryIdentifier id, CancellationToken stoppingToken);
    Task<bool> CategoryExistsAsync(string name, CancellationToken stoppingToken);
    Task AddAsync(ProjectCategory category, CancellationToken stoppingToken);
}