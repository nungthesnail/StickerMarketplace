using Marketplace.Core.Abstractions;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class ProjectCategory : IEntity<CategoryIdentifier>
{
    public CategoryIdentifier Id { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
