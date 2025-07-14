using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ProjectCreationUserState : FormUserState<ProjectCreationProgress>
{
    public CategoryIdentifier Category { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectDescription { get; set; }
    public string? ProjectImageId { get; set; }
    public string? ProjectContentUrl { get; set; }
    public long? ProjectTagId { get; set; }
    
    public override void Reset()
    {
        Category = default;
        ProjectName = null;
        ProjectDescription = null;
        ProjectImageId = null;
        ProjectContentUrl = null;
        ProjectTagId = default;
        Progress = default;
    }

    public override bool Completed => Progress == ProjectCreationProgress.Completed;
    public override void MoveProgressNext() => Progress = Progress.GetNext();
    public override void MoveProgressBack() => Progress = Progress.GetPrevious();
    public override ProjectCreationProgress Progress { get; set; }
}
