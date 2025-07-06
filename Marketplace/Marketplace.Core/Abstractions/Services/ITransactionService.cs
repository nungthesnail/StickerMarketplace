using Marketplace.Core.Models;
using Marketplace.Core.Models.Enums;

namespace Marketplace.Core.Abstractions.Services;

public interface ITransactionService
{
    Task CreateTransactionAsync(Transaction transaction, CancellationToken stoppingToken = default);

    Task UpdateTransactionStatusAsync(Guid id, TransactionStatus status,
        CancellationToken stoppingToken = default);
}