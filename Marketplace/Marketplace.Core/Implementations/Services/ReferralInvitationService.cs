using System.Data;
using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Exceptions;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Settings;

namespace Marketplace.Core.Implementations.Services;

public class ReferralInvitationService(IUnitOfWork uow, ISubscriptionService subscriptionService,
    ReferralInvitationSettings settings) : IReferralInvitationService
{
    public async Task<bool> IsUserInvitedAsync(long userId, CancellationToken stoppingToken = default)
        => await uow.ReferralInvitationRepository.AnyAsync(x => x.InvitedUserId == userId, stoppingToken);
    
    public async Task CreateInvitationAsync(ReferralInvitation invitation, bool openTransaction = true,
        bool updateInvitations = true, CancellationToken stoppingToken = default)
    {
        if (openTransaction)
        {
            await uow.BeginTransactionAsync(
                isolationLevel: IsolationLevel.Serializable, // To prevent adding new ref while the transaction executes
                stoppingToken: stoppingToken);
        }

        var userInvited = await IsUserInvitedAsync(invitation.InvitedUserId, stoppingToken);
        if (userInvited)
        {
            if (openTransaction)
                await uow.RollbackTransactionAsync(stoppingToken);
            throw new UserAlreadyInvitedException(invitation.InvitedUserId.ToString());
        }
        
        await uow.ReferralInvitationRepository.AddAsync(invitation, stoppingToken);
        var renewInvitedSuccess = await RenewSubscriptionAsync(invitation.InvitedUserId, stoppingToken);
        if (!renewInvitedSuccess)
        {
            if (openTransaction)
                await uow.RollbackTransactionAsync(stoppingToken);
            throw new InvalidOperationException("Error while giving subscription for invitation. " +
                                                $"Invited user id={invitation.InvitingUserId}");
        }
        
        if (updateInvitations)
        {
            var success = await UpdateUserInvitationsAsync(invitation.InvitingUserId,
                openTransaction: false, stoppingToken: stoppingToken);
            if (!success)
            {
                if (openTransaction)
                    await uow.RollbackTransactionAsync(stoppingToken);
                throw new InvalidOperationException("Error while giving subscription for invitation. " +
                                                    $"Inviting user id={invitation.InvitedUserId}");
            }
        }
        
        await uow.SaveChangesAsync(stoppingToken);
        await uow.CommitTransactionAsync(stoppingToken);
    }

    private async Task<bool> RenewSubscriptionAsync(long userId, CancellationToken stoppingToken = default)
    {
        var subscriptionRenewInterval = TimeSpan.FromDays(settings.SubscriptionRenewDays);
        var subscription = await subscriptionService.GetByUserIdAsync(userId, stoppingToken);
        if (subscription is null)
            return false;

        await subscriptionService.RenewSubscriptionAsync(
            id: subscription.Id,
            timeSpan: subscriptionRenewInterval,
            enhanced: settings.IsSubscriptionEnhanced,
            stoppingToken: stoppingToken);
        return true;
    }
    
    /// <returns>Operation success</returns>
    public async Task<bool> UpdateUserInvitationsAsync(long userId, bool openTransaction = true,
        CancellationToken stoppingToken = default)
    {
        if (openTransaction)
            await uow.BeginTransactionAsync(stoppingToken: stoppingToken);
        
        var invitationsCount = await GetUserUncheckedInvitationsCountAsync(userId, stoppingToken);
        if (invitationsCount < settings.MinCountToRenewSubscription)
        {
            if (openTransaction)
                await uow.RollbackTransactionAsync(stoppingToken);
            return false;
        }

        var success = await RenewSubscriptionAsync(userId, stoppingToken);
        if (!success)
        {
            if (openTransaction)
                await uow.RollbackTransactionAsync(stoppingToken);
            return false;
        }
        await MarkUserInvitationsCheckedAsync(userId, stoppingToken);
        
        await uow.SaveChangesAsync(stoppingToken);
        if (openTransaction)
            await uow.CommitTransactionAsync(stoppingToken);
        return true;
    }

    private async Task<int> GetUserUncheckedInvitationsCountAsync(long userId, CancellationToken stoppingToken = default)
        => await uow.ReferralInvitationRepository.CountAsync(
            x => x.InvitingUserId == userId && !x.SubscriptionRenewed,
            stoppingToken);

    private async Task MarkUserInvitationsCheckedAsync(long userId, CancellationToken stoppingToken = default)
    {
        await uow.ReferralInvitationRepository.UpdateByAsync(
            predicate: x => x.InvitingUserId == userId && !x.SubscriptionRenewed,
            propertySelector: x => x.SubscriptionRenewed,
            valueSelector: _ => true,
            stoppingToken: stoppingToken);
    }
    
    public async Task<IEnumerable<ReferralInvitation>> GetUserInvitations(long userId, bool onlyUnchecked = false,
        CancellationToken stoppingToken = default)
        => await uow.ReferralInvitationRepository.GetByAsync(
            predicate: x => x.InvitingUserId == userId && ((onlyUnchecked && !x.SubscriptionRenewed) || !onlyUnchecked),
            stoppingToken: stoppingToken);
}
