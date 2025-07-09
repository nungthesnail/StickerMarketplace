using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class Promocode : IEntity<long>
{
    public long Id { get; init; }
    public required string Text { get; set; }
    public DateTimeOffset ActiveUntil { get; set; }
    public double SubscriptionRenewDays { get; set; }
    public bool IsRenewEnhanced { get; set; }

    public List<PromocodeActivation>? Activations { get; set; } = [];
}
