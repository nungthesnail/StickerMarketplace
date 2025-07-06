using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class Like : IEntity<long>
{
    public long Id { get; init; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public long ProjectId { get; set; }
    public Project? Project { get; set; }
    public bool Liked { get; set; }
}
