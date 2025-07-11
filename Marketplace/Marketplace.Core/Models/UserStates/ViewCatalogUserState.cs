using Marketplace.Core.Models.Catalog;

namespace Marketplace.Core.Models.UserStates;

public class ViewCatalogUserState : UserState
{
    public CatalogProjectView ProjectView { get; set; } = new();
    public CatalogFilter? Filter { get; set; }
    
    public override void Reset()
    {
        ProjectView = new();
        Filter = new CatalogFilter();
    }
}
