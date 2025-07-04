namespace Marketplace.Core.Models.UserStates;

public interface IUserState
{
    long UserId { get; init; }
    void Reset();
}
