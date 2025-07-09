using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Exceptions;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Settings;

namespace Marketplace.Core.Implementations.Services;

public class SubscriptionService(IUnitOfWork uow, SubscriptionSettings settings) : ISubscriptionService
{
    public async Task<Subscription?> GetByIdAsync(long id, CancellationToken stoppingToken = default)
        => await uow.SubscriptionRepository.GetByIdAsync(id, stoppingToken);
    
    public async Task<Subscription?> GetByUserIdAsync(long userId, CancellationToken stoppingToken = default)
        => (await uow.SubscriptionRepository.GetByAsync(x => x.UserId == userId, stoppingToken: stoppingToken))
            .FirstOrDefault();
    
    public async Task<bool> DoesUserHaveSubscriptionAsync(long userId, CancellationToken stoppingToken = default)
        => await uow.SubscriptionRepository.AnyAsync(x => x.UserId == userId, stoppingToken);
    
    public async Task AddSubscriptionAsync(Subscription subscription, CancellationToken stoppingToken = default)
    {
        await uow.BeginTransactionAsync(stoppingToken: stoppingToken);
        var userHaveSubscription = await DoesUserHaveSubscriptionAsync(subscription.UserId, stoppingToken);
        if (userHaveSubscription)
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw new UserAlreadyHaveSubscriptionException(subscription.UserId.ToString());
        }
        await uow.SubscriptionRepository.AddAsync(subscription, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }

    public async Task UpdateCachedActivationAsync(CancellationToken stoppingToken = default)
    {
        var now = DateTimeOffset.Now;
        await uow.SubscriptionRepository.UpdateByAsync(
            predicate: _ => true,
            propertySelector: x => x.Active,
            valueSelector: x => x.BaseActiveUntil > now || x.EnhancedUntil > now,
            stoppingToken: stoppingToken);
    }

    public async Task RenewSubscriptionAsync(long id, TimeSpan timeSpan, bool enhanced = false,
        CancellationToken stoppingToken = default)
    {
        var now = DateTimeOffset.Now;
        Func<Subscription, DateTimeOffset?> propertySelector = x => x.BaseActiveUntil;
        if (enhanced)
        {
            propertySelector = x => x.EnhancedUntil;
        }
        
        await uow.SubscriptionRepository.UpdateByAsync(
            predicate: x => x.Id == id,
            propertySelector: propertySelector,
            valueSelector: _ => now + timeSpan,
            stoppingToken: stoppingToken);
    }

    public SubscriptionPriceInfo[] GetPrices() => settings.PriceInfos;
}
