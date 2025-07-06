using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Services;

public interface IReferralInvitationService
{
    Task<bool> IsUserInvitedAsync(long userId, CancellationToken stoppingToken = default);

    Task CreateInvitationAsync(ReferralInvitation invitation, bool openTransaction = true,
        bool updateInvitations = true, CancellationToken stoppingToken = default);

    /// <returns>Operation success</returns>
    Task<bool> UpdateUserInvitationsAsync(long userId, bool openTransaction = true,
        CancellationToken stoppingToken = default);

    Task<IEnumerable<ReferralInvitation>> GetUserInvitations(long userId, bool onlyUnchecked = false,
        CancellationToken stoppingToken = default);
}