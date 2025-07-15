using System.Collections.Concurrent;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models.UserStates;

namespace Marketplace.InMemoryCache.Services;

public class InMemoryUserStateService : IUserStateService
{
    private readonly ConcurrentDictionary<long, UserState> _statesStorage = new();
    
    public UserState? GetUserState(long userId)
        => _statesStorage.GetValueOrDefault(userId);
    public void SetUserState(long userId, UserState state)
        => _statesStorage.AddOrUpdate(userId, state, (_, _) => state);
    public void RemoveUserState(long userId)
        => _statesStorage.TryRemove(userId, out _);
}
