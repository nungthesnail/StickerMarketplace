namespace Marketplace.Core.Models.UserStates;

public class DefaultUserState : IUserState
{
    public long UserId { get; init; }
    public void Reset()
    { }
}
