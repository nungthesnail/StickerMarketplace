using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class ProjectTag
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public long? CreatedByUserId { get; set; }
    public CategoryIdentifier Category { get; set; }
}
