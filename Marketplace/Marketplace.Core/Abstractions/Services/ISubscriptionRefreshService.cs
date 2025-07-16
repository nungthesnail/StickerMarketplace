namespace Marketplace.Core.Abstractions.Services;

public interface ISubscriptionRefreshService
{
    Task UpdateSubscriptionsAsync(CancellationToken stoppingToken = default);
}
