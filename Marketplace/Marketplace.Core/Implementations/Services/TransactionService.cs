using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Implementations.Services;

public class TransactionService(IUnitOfWork uow) : ITransactionService
{
    public async Task CreateTransactionAsync(Transaction transaction, CancellationToken stoppingToken = default)
    {
        await uow.TransactionRepository.AddAsync(transaction, stoppingToken);
        await uow.SaveChangesAsync(stoppingToken);
    }

    public async Task UpdateTransactionStatusAsync(Guid id, TransactionStatus status,
        CancellationToken stoppingToken = default)
    {
        await uow.TransactionRepository.UpdateByAsync(x => x.Id == id, x => x.Status, _ => status, stoppingToken);
    }
}
