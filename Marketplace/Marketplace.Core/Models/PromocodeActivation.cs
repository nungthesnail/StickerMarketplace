using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class PromocodeActivation : IEntity<long>
{
    public long Id { get; init; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public long PromocodeId { get; set; }
    public Promocode? Promocode { get; set; }
    public DateTimeOffset ActivatedAt { get; set; }
}
