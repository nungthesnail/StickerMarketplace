using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Services;

public interface ISubscriptionService
{
    Task<Subscription?> GetByIdAsync(long id, CancellationToken stoppingToken = default);
    Task<Subscription?> GetByUserIdAsync(long userId, CancellationToken stoppingToken = default);
    Task<bool> DoesUserHaveSubscriptionAsync(long userId, CancellationToken stoppingToken = default);
    Task AddSubscriptionAsync(Subscription subscription, CancellationToken stoppingToken = default);
    Task UpdateCachedActivationAsync(CancellationToken stoppingToken = default);
    Task RenewSubscriptionAsync(long id, TimeSpan timeSpan, bool enhanced = false,
        CancellationToken stoppingToken = default);
    Task RenewSubscriptionByUserIdAsync(long userId, TimeSpan timeSpan, bool enhanced = false,
        CancellationToken stoppingToken = default);
    SubscriptionPriceInfo[] GetPrices();
}
