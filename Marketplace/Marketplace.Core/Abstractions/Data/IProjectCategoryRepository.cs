using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Abstractions.Data;

public interface IProjectCategoryRepository : IRepository<ProjectCategory, CategoryIdentifier>;
