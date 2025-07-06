using System.Data;
using System.Diagnostics.CodeAnalysis;
using Marketplace.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Marketplace.EntityFrameworkCore.Implementations;

public abstract class EfBaseUnitOfWork(DbContext dbContext) : IBaseUnitOfWork
{
    private IDbContextTransaction? _transaction;
    
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken stoppingToken = default)
    {
        ThrowIfTransactionIsNotNull();
        _transaction = await dbContext.Database.BeginTransactionAsync(isolationLevel, stoppingToken);
    }

    private void ThrowIfTransactionIsNotNull()
    {
        if (_transaction is not null)
            throw new InvalidOperationException($"There's an opened transaction: {_transaction}");
    }

    public async Task CreateTransactionSavepointAsync(string savepointName, CancellationToken stoppingToken = default)
    {
        ThrowIfTransactionIsNull();
        ThrowIfTransactionDoesntSupportSavePoints();
        await _transaction.CreateSavepointAsync(savepointName, stoppingToken);
    }

    [MemberNotNull(nameof(_transaction))]
    private void ThrowIfTransactionIsNull()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction");
    }

    private void ThrowIfTransactionDoesntSupportSavePoints()
    {
        if (_transaction?.SupportsSavepoints ?? false)
            throw new InvalidOperationException("Transaction doesn't support save points");
    }

    public async Task CommitTransactionAsync(CancellationToken stoppingToken = default)
    {
        ThrowIfTransactionIsNull();
        await _transaction.CommitAsync(stoppingToken);
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken stoppingToken = default)
    {
        ThrowIfTransactionIsNull();
        await _transaction.RollbackAsync(stoppingToken);
        _transaction = null;
    }

    public async Task RollbackTransactionToSavepointAsync(string savepointName,
        CancellationToken stoppingToken = default)
    {
        ThrowIfTransactionIsNull();
        ThrowIfTransactionDoesntSupportSavePoints();
        await _transaction.RollbackToSavepointAsync(savepointName, stoppingToken);
            
    }

    public async Task ReleaseTransactionSavepointAsync(string savepointName, CancellationToken stoppingToken = default)
    {
        ThrowIfTransactionIsNull();
        ThrowIfTransactionDoesntSupportSavePoints();
        await _transaction.ReleaseSavepointAsync(savepointName, stoppingToken);
    }

    public Task SaveChangesAsync(CancellationToken stoppingToken = default)
        => dbContext.SaveChangesAsync(stoppingToken);
    
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();
        await dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
