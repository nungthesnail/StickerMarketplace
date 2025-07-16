using System.Data;
using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;

namespace Marketplace.Core.Implementations.Services;

public class SubscriptionRefreshService(IUnitOfWork uow) : ISubscriptionRefreshService
{
    public async Task UpdateSubscriptionsAsync(CancellationToken stoppingToken = default)
    {
        await uow.BeginTransactionAsync(IsolationLevel.Serializable, stoppingToken);
        var now = DateTimeOffset.Now;
        await uow.SubscriptionRepository.UpdateByAsync(
            predicate: x => now > x.EnhancedUntil || now > x.BaseActiveUntil,
            propertySelector: x => x.Active,
            valueSelector: _ => false,
            stoppingToken: stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }
}
