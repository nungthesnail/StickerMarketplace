using Marketplace.Core.Models.Catalog;

namespace Marketplace.Core.Models.UserStates;

public class ViewCatalogUserState : IUserState
{
    public long UserId { get; init; }
    public required CatalogProjectView ProjectView { get; set; }
    public required CatalogFilter Filter { get; set; }
    
    public void Reset()
    {
        ProjectView = new();
        Filter = new CatalogFilter();
    }
}
