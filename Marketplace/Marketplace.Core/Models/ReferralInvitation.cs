using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class ReferralInvitation : IEntity<long>
{
    public long Id { get; init; }
    public long InvitedUserId { get; set; } // Accepts invitation
    public User? InvitedUser { get; set; }
    public long InvitingUserId { get; set; } // Issues invitation
    public User? InvitingUser { get; set; }
    public DateTimeOffset InvitedAt { get; set; }
    public bool SubscriptionRenewed { get; set; }
}
