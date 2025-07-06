using Marketplace.Core.Models.Catalog;
using Marketplace.Core.Models.Enums;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ProjectSearchUserState : IFormUserState<ProjectSearchProgress>
{
    public long UserId { get; init; }
    public CatalogFilter Filter { get; set; } = new();
    public void Reset()
    {
        Filter = new CatalogFilter();
        Progress = default;
    }

    public bool Completed => Progress == ProjectSearchProgress.Completed;
    public void MoveProgressNext() => Progress = Progress.GetNext();
    public void MoveProgressBack() => Progress = Progress.GetPrevious();
    public ProjectSearchProgress Progress { get; private set; }
}
