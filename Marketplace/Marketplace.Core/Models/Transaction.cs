using Marketplace.Core.Abstractions;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models;

public class Transaction : IEntity<Guid>
{
    public Guid Id { get; init; }
    public long UserId { get; init; }
    public User? User { get; init; }
    public decimal Amount { get; init; }
    public TransactionPurpose Purpose { get; init; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? FinishedAt { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Created;
}
