using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class Subscription : IEntity<long>
{
    public long Id { get; init; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public bool Active { get; set; }
    public DateTimeOffset? BaseActiveUntil { get; set; }
    public DateTimeOffset? EnhancedUntil { get; set; }
}
