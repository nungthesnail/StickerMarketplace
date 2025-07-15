using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Models.Catalog;

public class CatalogFilter
{
    public CategoryIdentifier Category { get; set; }
    public List<long> TagIds { get; set; } = [];
    public bool NotModerated { get; set; }
}
