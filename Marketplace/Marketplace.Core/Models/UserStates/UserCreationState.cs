using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class UserCreationState : FormUserState<UserCreationProgress>
{
    public string? PromoCode { get; set; } // Don't reset these two fields because they are an external-provided data.
    public long? InvitedByUserId { get; set; }
    public string? UserName { get; set; }
    
    public override void Reset()
    {
        Progress = default;
        UserName = null;
    }

    public override bool Completed => Progress == UserCreationProgress.Completed;
    public override void MoveProgressNext() => Progress = Progress.GetNext();
    public override void MoveProgressBack() => Progress = Progress.GetPrevious();
    public override UserCreationProgress Progress { get; set; }
}
