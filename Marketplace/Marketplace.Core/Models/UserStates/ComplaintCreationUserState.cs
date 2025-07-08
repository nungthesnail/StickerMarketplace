using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ComplaintCreationUserState : FormUserState<ComplaintCreationProgress>
{
    public long? ViolatorUserId { get; set; }
    public long? ProjectId { get; set; }
    public ComplaintTopic Topic { get; set; }
    public string? Text { get; set; }
    
    public override void Reset()
    {
        ViolatorUserId = default;
        ProjectId = default;
        Topic = default;
        Text = null;
        Progress = default;
    }

    public override bool Completed => Progress == ComplaintCreationProgress.Completed;
    public override void MoveProgressNext() => Progress = Progress.GetNext();
    public override void MoveProgressBack() => Progress = Progress.GetPrevious();
    public override ComplaintCreationProgress Progress { get; set; }
}
