using System.Data;
using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;

namespace Marketplace.Core.Implementations.Services;

public class LikeService(IUnitOfWork uow) : ILikeService
{
    public async Task<bool?> GetProjectLikeByUserAsync(long userId, long projectId, CancellationToken stoppingToken)
    {
        return (await uow.LikeRepository.GetByAsync(
            predicate: x => x.ProjectId == projectId && x.UserId == userId,
            stoppingToken: stoppingToken))
            .FirstOrDefault()?
            .Liked;
    }

    public async Task LikeProjectAsync(long userId, long projectId, bool isLiked, CancellationToken stoppingToken)
    {
        await uow.BeginTransactionAsync(isolationLevel: IsolationLevel.ReadUncommitted, stoppingToken: stoppingToken);
        var like = await GetProjectLikeByUserAsync(userId, projectId, stoppingToken);
        if (like == isLiked)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            return;
        }

        if (like is null)
        {
            await uow.LikeRepository.AddAsync(new Like
                {
                    UserId = userId,
                    ProjectId = projectId,
                    Liked = isLiked
                },
                stoppingToken);
            await uow.SaveChangesAsync(stoppingToken);
        }
        else
        {
            await uow.LikeRepository.UpdateByAsync(
                predicate: x => x.ProjectId == projectId && x.UserId == userId,
                propertySelector: x => x.Liked,
                valueSelector: _ => isLiked,
                stoppingToken: stoppingToken);
        }
        await uow.CommitTransactionAsync(stoppingToken);
    }
}
