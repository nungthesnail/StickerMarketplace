using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Services;

public interface IUserService
{
    Task<bool> IsNameAvailableAsync(string userName, CancellationToken stoppingToken = default);
    Task<bool> IsUserRegisteredAsync(long userId, CancellationToken stoppingToken = default);

    Task CreateUserAsync(User user, Subscription? subscription = null, bool openTransaction = true,
        CancellationToken stoppingToken = default);

    Task<User?> GetUserByIdAsync(long userId, bool includeSubscription = false,
        CancellationToken stoppingToken = default);
    Task<User?> GetUserByNameAsync(string name, CancellationToken stoppingToken = default);
}