using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class UserCreationState : IFormUserState<UserCreationProgress>
{
    public long UserId { get; init; }
    public string? PromoCode { get; set; } // Don't reset these two fields because they are an external-provided data.
    public string? InvitedByUserId { get; set; }
    public string? UserName { get; set; }
    
    public void Reset()
    {
        Progress = default;
        UserName = null;
    }

    public bool Completed => Progress == UserCreationProgress.Completed;
    public void MoveProgressNext() => Progress = Progress.GetNext();
    public void MoveProgressBack() => Progress = Progress.GetPrevious();
    public UserCreationProgress Progress { get; private set; }
}
