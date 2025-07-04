using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class Complaint
{
    public long Id { get; init; }
    public long CreatedByUserId { get; set; }
    public long? ViolatorUserId { get; set; }
    public long? ProjectId { get; set; }
    public ComplaintTopic Topic { get; set; }
    public required string Content { get; set; }
    public bool Reviewed { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}
