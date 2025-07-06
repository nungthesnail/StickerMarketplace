using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class User : IEntity<long>
{
    public long Id { get; init; }
    public required string Name { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public long? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public bool IsAdmin { get; set; } = false;
}
