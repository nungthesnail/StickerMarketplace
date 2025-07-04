namespace Marketplace.Core.Models;

public class Subscription
{
    public int Id { get; init; }
    public long UserId { get; set; }
    public DateTimeOffset? ActiveUntil => EnhancedUntil > BaseActiveUntil ? EnhancedUntil : BaseActiveUntil;
    public DateTimeOffset? BaseActiveUntil { get; set; }
    public DateTimeOffset? EnhancedUntil { get; set; }
}
