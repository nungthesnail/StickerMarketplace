using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Exceptions;
using Marketplace.Core.Models;

namespace Marketplace.Core.Implementations.Services;

public class ProjectTagService(IUnitOfWork uow) : IProjectTagService
{
    public async Task<bool> IsNameAvailableAsync(string name, CancellationToken stoppingToken)
        => !await uow.ProjectTagRepository.AnyAsync(x => x.Name == name, stoppingToken);

    public async Task CreateTagAsync(ProjectTag tag, CancellationToken stoppingToken)
    {
        await uow.BeginTransactionAsync(stoppingToken: stoppingToken);
        var nameAvailable = await IsNameAvailableAsync(tag.Name, stoppingToken);
        if (!nameAvailable)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw new TagAlreadyExistsException(tag.Name);
        }
        await uow.ProjectTagRepository.AddAsync(tag, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }

    public async Task GetTagsAsync(CancellationToken stoppingToken)
        => await uow.ProjectTagRepository.GetAllAsync(stoppingToken);
}
