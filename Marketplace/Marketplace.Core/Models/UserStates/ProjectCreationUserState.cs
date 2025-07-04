using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ProjectCreationUserState : IFormUserState<ProjectCreationProgress>
{
    public long UserId { get; init; }
    
    public CategoryIdentifier Category { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectDescription { get; set; }
    public long ProjectImageId { get; set; }
    public string? ProjectContentUrl { get; set; }
    public ProjectTag? ProjectTag { get; set; }
    
    public void Reset()
    {
        Category = default;
        ProjectName = null;
        ProjectDescription = null;
        ProjectImageId = default;
        ProjectContentUrl = null;
        ProjectTag = null;
        Progress = default;
    }

    public bool Completed => Progress == ProjectCreationProgress.Completed;
    public void MoveProgressNext() => Progress = Progress.GetNext();
    public void MoveProgressBack() => Progress = Progress.GetPrevious();
    public ProjectCreationProgress Progress { get; private set; }
}
