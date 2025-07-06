using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Exceptions;
using Marketplace.Core.Models;

namespace Marketplace.Core.Implementations.Services;

public class UserService(IUnitOfWork uow) : IUserService
{
    public async Task<bool> IsNameAvailableAsync(string userName, CancellationToken stoppingToken = default)
        => !await uow.UserRepository.AnyAsync(x => x.Name == userName, stoppingToken);
    
    public async Task<bool> IsUserRegisteredAsync(long userId, CancellationToken stoppingToken = default)
        => await uow.UserRepository.AnyAsync(x => x.Id == userId, stoppingToken);

    public async Task CreateUserAsync(User user, Subscription? subscription = null, bool openTransaction = true,
        CancellationToken stoppingToken = default)
    {
        if (openTransaction)
            await uow.BeginTransactionAsync(stoppingToken: stoppingToken);
        var nameAvailable = await IsNameAvailableAsync(user.Name, stoppingToken);
        var userRegistered = await IsUserRegisteredAsync(user.Id, stoppingToken);
        
        if (!nameAvailable)
        {
            if (openTransaction)
                await uow.RollbackTransactionAsync(stoppingToken);
            throw new UserNameAlreadyExistsException(user.Name);
        }
        if (userRegistered)
        {
            if (openTransaction)
                await uow.RollbackTransactionAsync(stoppingToken);
            throw new UserIdAlreadyExistsException(user.Id.ToString());
        }
        
        await uow.UserRepository.AddAsync(user, stoppingToken);
        
        if (subscription is not null)
            await uow.SubscriptionRepository.AddAsync(subscription, stoppingToken);
        
        await uow.SaveChangesAsync(stoppingToken);
        if (openTransaction)
            await uow.CommitTransactionAsync(stoppingToken);
    }
    
    public async Task<User?> GetUserByIdAsync(long userId, CancellationToken stoppingToken = default)
        => await uow.UserRepository.GetByIdAsync(userId, stoppingToken);
    
    public async Task<User?> GetUserByNameAsync(string name, CancellationToken stoppingToken = default)
        => (await uow.UserRepository.GetByAsync(x => x.Name == name, stoppingToken: stoppingToken)).FirstOrDefault();
}
