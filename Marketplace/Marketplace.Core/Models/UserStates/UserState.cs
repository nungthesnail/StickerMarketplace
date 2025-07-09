namespace Marketplace.Core.Models.UserStates;

public abstract class UserState
{
    public long UserId { get; init; }
    public int? LastMessageId { get; set; }
    public abstract void Reset();
}
