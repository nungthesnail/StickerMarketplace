namespace Marketplace.Core.Models;

public class Project
{
    public long Id { get; init; }
    public long UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public long? ImageId { get; set; }
    public string? ContentUrl { get; set; }
    public bool Moderated { get; set; } = false;
    
    public required ProjectTag Tag { get; set; }
    public required ProjectCategory Category { get; set; }
}
