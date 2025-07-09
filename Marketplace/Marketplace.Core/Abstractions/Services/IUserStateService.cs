using Marketplace.Core.Models.UserStates;

namespace Marketplace.Core.Abstractions.Services;

public interface IUserStateService
{
    UserState? GetUserState(long userId);
    void SetUserState(long userId, UserState state);
    void RemoveUserState(long userId);
}
