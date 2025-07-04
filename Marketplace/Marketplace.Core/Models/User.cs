namespace Marketplace.Core.Models;

public class User
{
    public long Id { get; init; }
    public required string Name { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public long? InvitedByUserId { get; set; }
    public long? SubscriptionId { get; set; }
    public bool IsAdmin { get; set; } = false;
}
