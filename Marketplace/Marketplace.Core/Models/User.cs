using Marketplace.Core.Abstractions;

namespace Marketplace.Core.Models;

public class User : IEntity<long>
{
    public long Id { get; init; }
    public required string Name { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public long? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public bool IsAdmin { get; set; }
    public long? InvitationId { get; set; }
    public ReferralInvitation? Invitation { get; set; }
    
    public List<ReferralInvitation>? MyInvitations { get; set; }
    public List<Transaction>? Transactions { get; set; }
    public List<Project>? Projects { get; set; } = [];
    public List<Complaint>? IssuedComplaints { get; set; } = [];
    public List<Complaint>? AccusingComplaints { get; set; } = [];
    public List<Like>? Likes { get; set; } = [];
    public List<ProjectTag>? ProjectTags { get; set; }
}
