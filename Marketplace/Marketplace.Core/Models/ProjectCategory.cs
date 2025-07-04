using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class ProjectCategory
{
    public CategoryIdentifier Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
