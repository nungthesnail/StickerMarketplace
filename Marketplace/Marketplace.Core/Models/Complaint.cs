using Marketplace.Core.Abstractions;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class Complaint : IEntity<long>
{
    public long Id { get; init; }
    public long CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public long? ViolatorUserId { get; set; }
    public User? ViolatorUser { get; set; }
    public long? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public ComplaintTopic Topic { get; set; }
    public required string Content { get; set; }
    public bool Reviewed { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ReviewedAt { get; set; }
}
