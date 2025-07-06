using Marketplace.Core.Models;

namespace Marketplace.Core.Abstractions.Data;

public interface ITransactionRepository : IRepository<Transaction, Guid>;
