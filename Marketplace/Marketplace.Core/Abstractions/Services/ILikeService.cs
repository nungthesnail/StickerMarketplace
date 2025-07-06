namespace Marketplace.Core.Abstractions.Services;

public interface ILikeService
{
    Task<bool?> GetProjectLikeByUserAsync(long userId, long projectId, CancellationToken stoppingToken);
    Task LikeProjectAsync(long userId, long projectId, bool isLiked, CancellationToken stoppingToken);
}