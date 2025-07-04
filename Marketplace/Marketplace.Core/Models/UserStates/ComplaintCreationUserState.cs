using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ComplaintCreationUserState : IFormUserState<ComplaintCreationProgress>
{
    public long UserId { get; init; }
    public long? ViolatorUserId { get; set; }
    public long? ProjectId { get; set; }
    public ComplaintTopic Topic { get; set; }
    public string? Text { get; set; }
    
    public void Reset()
    {
        ViolatorUserId = default;
        ProjectId = default;
        Topic = default;
        Text = null;
        Progress = default;
    }

    public bool Completed => Progress == ComplaintCreationProgress.Completed;
    public void MoveProgressNext() => Progress = Progress.GetNext();
    public void MoveProgressBack() => Progress = Progress.GetPrevious();
    public ComplaintCreationProgress Progress { get; private set; }
}
