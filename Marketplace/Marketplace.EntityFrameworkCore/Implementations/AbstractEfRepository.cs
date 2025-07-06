using Marketplace.Core.Abstractions;
using Marketplace.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.EntityFrameworkCore.Implementations;

public abstract class AbstractEfRepository<TEntity, TKey>(AppDbContext dbContext)
    : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    public async Task AddAsync(TEntity entity, CancellationToken stoppingToken)
        => await dbContext.AddAsync(entity, stoppingToken);

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken stoppingToken)
        => await dbContext.FindAsync<TEntity>([id], cancellationToken: stoppingToken);

    public IAsyncEnumerable<TEntity> GetAllAsync(CancellationToken stoppingToken)
        => dbContext.Set<TEntity>().AsAsyncEnumerable();

    public void Update(TEntity entity)
        => dbContext.Update(entity);

    public void Delete(TEntity entity)
        => dbContext.Remove(entity);

    public async Task<IEnumerable<TEntity>> GetByAsync(
        Func<TEntity, bool> predicate,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default)
    {
        var set = GetSetAndJoinProperties(joins);
        return await set
            .AsEnumerable()
            .Where(predicate)
            .AsQueryable()
            .ToListAsync(stoppingToken);
    }

    private IQueryable<TEntity> GetSetAndJoinProperties(IEnumerable<string>? joins)
    {
        var set = dbContext.Set<TEntity>().AsQueryable();
        joins ??= [];
        return joins.Aggregate(set, (current, joinProperty) => current.Include(joinProperty));
    }
    
    public async Task<IEnumerable<TEntity>> GetByAsync<TOrderProperty>(
        Func<TEntity, bool> predicate,
        Func<TEntity, TOrderProperty> orderBy,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default)
    {
        var set = GetSetAndJoinProperties(joins);
        return await set
            .AsEnumerable()
            .Where(predicate)
            .OrderBy(orderBy)
            .AsQueryable()
            .ToListAsync(stoppingToken);
    }

    public async Task UpdateByAsync<TProperty>(
        Func<TEntity, bool> predicate,
        Func<TEntity, TProperty> propertySelector,
        Func<TEntity, TProperty> valueSelector,
        CancellationToken stoppingToken)
    {
        await dbContext
            .Set<TEntity>()
            .Where(predicate)
            .AsQueryable()
            .ExecuteUpdateAsync(
                propertySetter => propertySetter.SetProperty(propertySelector, valueSelector),
                stoppingToken);
    }

    public async Task DeleteByAsync(Func<TEntity, bool> predicate, CancellationToken stoppingToken)
    {
        await dbContext
            .Set<TEntity>()
            .Where(predicate)
            .AsQueryable()
            .ExecuteDeleteAsync(stoppingToken);
    }
}
