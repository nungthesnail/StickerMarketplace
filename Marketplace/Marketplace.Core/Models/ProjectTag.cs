using Marketplace.Core.Abstractions;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class ProjectTag : IEntity<long>
{
    public long Id { get; init; }
    public required string Name { get; set; }
    public long? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public List<Project>? Projects { get; set; } = [];
}
