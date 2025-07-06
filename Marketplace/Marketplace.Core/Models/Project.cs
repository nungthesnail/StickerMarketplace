using Marketplace.Core.Abstractions;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class Project : IEntity<long>
{
    public long Id { get; init; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public long? ImageId { get; set; }
    public string? ContentUrl { get; set; }
    public bool Moderated { get; set; }
    public bool Visible { get; set; } = true;
    public long TagId { get; set; }
    public ProjectTag? Tag { get; set; }
    public CategoryIdentifier CategoryId { get; set; }
    public ProjectCategory? Category { get; set; }

    public List<Like>? Likes { get; set; } = [];
    public List<Complaint> Complaints { get; set; } = [];
}
