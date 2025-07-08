using Marketplace.Core.Models.Catalog;
using Marketplace.Core.Models.Enums.UserStates;

namespace Marketplace.Core.Models.UserStates;

public class ProjectSearchUserState : FormUserState<ProjectSearchProgress>
{
    public CatalogFilter Filter { get; set; } = new();
    public override void Reset()
    {
        Filter = new CatalogFilter();
        Progress = default;
    }

    public override bool Completed => Progress == ProjectSearchProgress.Completed;
    public override void MoveProgressNext() => Progress = Progress.GetNext();
    public override void MoveProgressBack() => Progress = Progress.GetPrevious();
    public override ProjectSearchProgress Progress { get; set; }
}
