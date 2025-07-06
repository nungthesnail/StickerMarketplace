using Marketplace.Core.Models.Catalog;

namespace Marketplace.Core.Models.UserStates;

public class ViewCatalogUserState : IUserState
{
    public long UserId { get; init; }
    public int Position { get; set; }
    public required CatalogFilter Filter { get; set; }
    
    public void Reset()
    {
        Position = 0;
        Filter = new CatalogFilter();
    }
}
