using System.Data;

namespace Marketplace.Core.Abstractions.Data;

public interface IBaseUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken stoppingToken = default);
    
    Task CreateTransactionSavepointAsync(string savepointName, CancellationToken stoppingToken = default);
    Task CommitTransactionAsync(CancellationToken stoppingToken = default);
    Task RollbackTransactionAsync(CancellationToken stoppingToken = default);
    Task RollbackTransactionToSavepointAsync(string savepointName, CancellationToken stoppingToken = default);
    Task ReleaseTransactionSavepointAsync(string savepointName, CancellationToken stoppingToken = default);
    Task SaveChangesAsync(CancellationToken stoppingToken = default);
}
