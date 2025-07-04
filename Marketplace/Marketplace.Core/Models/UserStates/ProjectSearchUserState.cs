using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ProjectSearchUserState : IFormUserState<ProjectSearchProgress>
{
    public long UserId { get; init; }
    public CategoryIdentifier Category { get; set; }
    public List<long> TagIds { get; set; } = [];
    
    public void Reset()
    {
        Category = default;
        TagIds.Clear();
        Progress = default;
    }

    public bool Completed => Progress == ProjectSearchProgress.Completed;
    public void MoveProgressNext() => Progress = Progress.GetNext();
    public void MoveProgressBack() => Progress = Progress.GetPrevious();
    public ProjectSearchProgress Progress { get; private set; }
}
