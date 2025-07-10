using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Exceptions;
using Marketplace.Core.Models;

namespace Marketplace.Core.Implementations.Services;

public class PromocodeService(IUnitOfWork uow, ISubscriptionService subscriptionService) : IPromocodeService
{
    public async Task CreatePromocodeAsync(Promocode promocode, CancellationToken stoppingToken = default)
    {
        await uow.BeginTransactionAsync(stoppingToken: stoppingToken);

        if (await PromocodeExistsAsync(promocode.Text, stoppingToken: stoppingToken))
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            throw new PromocodeAlreadyExistsException(promocode.Text);
        }

        await uow.PromocodeRepository.AddAsync(promocode, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }

    public async Task<bool> TryActivatePromocodeAsync(long userId, string text,
        CancellationToken stoppingToken = default)
    {
        await uow.BeginTransactionAsync(stoppingToken: default);
        
        var promocode = await GetPromocodeAsync(text, stoppingToken);
        if (promocode is null || await UserActivatedPromocodeAsync(userId, text, stoppingToken))
        {
            await uow.RollbackTransactionAsync(stoppingToken);
            return false;
        }
        
        await uow.PromocodeActivationRepository.AddAsync(new PromocodeActivation
        {
            UserId = userId,
            PromocodeId = promocode.Id,
            ActivatedAt = DateTimeOffset.Now
        }, stoppingToken);
        
        await ActivateSubscriptionAsync(
            userId, promocode.SubscriptionRenewDays, promocode.IsRenewEnhanced, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
        
        return true;
    }

    private async Task ActivateSubscriptionAsync(long userId, double days, bool enhanced,
        CancellationToken stoppingToken = default)
    {
        var interval = TimeSpan.FromDays(days);
        await subscriptionService.RenewSubscriptionByUserIdAsync(userId, interval, enhanced, stoppingToken);
    }

    public async Task<Promocode?> GetPromocodeAsync(string text, CancellationToken stoppingToken = default)
        => (await uow.PromocodeRepository.GetByAsync(x => x.Text == text, stoppingToken: stoppingToken))
            .FirstOrDefault();

    public async Task<IEnumerable<Promocode>> GetAllPromocodesAsync(CancellationToken stoppingToken = default)
        => await uow.PromocodeRepository.GetAllAsync(stoppingToken);

    public async Task<bool> PromocodeExistsAsync(string text, CancellationToken stoppingToken = default)
        => await uow.PromocodeRepository.AnyAsync(x => x.Text == text, stoppingToken);

    public async Task<bool> UserActivatedPromocodeAsync(long userId, string text,
        CancellationToken stoppingToken = default)
    {
        return await uow.PromocodeActivationRepository.AnyAsync(
            predicate: x => x.UserId == userId && x.Promocode!.Text == text,
            joins: [nameof(PromocodeActivation.Promocode)],
            stoppingToken: stoppingToken);
    }
}
