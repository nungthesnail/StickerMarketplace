using System.Linq.Expressions;
using Marketplace.Core.Abstractions;
using Marketplace.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.EntityFrameworkCore.Implementations;

public abstract class AbstractEfRepository<TEntity, TKey>(AppDbContext dbContext)
    : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    public async Task AddAsync(TEntity entity, CancellationToken stoppingToken = default)
        => await dbContext.AddAsync(entity, stoppingToken);

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken stoppingToken = default)
        => await dbContext.FindAsync<TEntity>([id], cancellationToken: stoppingToken);

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken stoppingToken)
        => await dbContext.Set<TEntity>().ToListAsync(stoppingToken);

    public void Update(TEntity entity)
        => dbContext.Update(entity);

    public void Delete(TEntity entity)
        => dbContext.Remove(entity);

    public async Task<IEnumerable<TEntity>> GetByAsync(
        Expression<Func<TEntity, bool>> predicate,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default)
    {
        var set = GetSetAndJoinProperties(joins);
        return await set
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
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TOrderProperty>> orderBy,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default)
    {
        var set = GetSetAndJoinProperties(joins);
        return await set
            .Where(predicate)
            .OrderBy(orderBy)
            .AsQueryable()
            .ToListAsync(stoppingToken);
    }

    public async Task<IEnumerable<TAggregation>> GetByGroupsAsync<TGroupKey, TAggregation>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TGroupKey>> keySelector,
        Expression<Func<IGrouping<TGroupKey, TEntity>, TAggregation>> groupSelector,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default)
    {
        var set = GetSetAndJoinProperties(joins);
        return await set
            .Where(predicate)
            .GroupBy(keySelector)
            .Select(groupSelector)
            .ToListAsync(stoppingToken);
    }

    public async Task UpdateByAsync<TProperty>(
        Expression<Func<TEntity, bool>> predicate,
        Func<TEntity, TProperty> propertySelector,
        Func<TEntity, TProperty> valueSelector,
        CancellationToken stoppingToken = default)
    {
        await dbContext
            .Set<TEntity>()
            .Where(predicate)
            .AsQueryable()
            .ExecuteUpdateAsync(
                propertySetter => propertySetter.SetProperty(propertySelector, valueSelector),
                stoppingToken);
    }

    public async Task DeleteByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken stoppingToken = default)
    {
        await dbContext
            .Set<TEntity>()
            .Where(predicate)
            .AsQueryable()
            .ExecuteDeleteAsync(stoppingToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken stoppingToken = default)
        => await dbContext.Set<TEntity>().AnyAsync(predicate, stoppingToken);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken stoppingToken = default)
        => await dbContext.Set<TEntity>().CountAsync(predicate, stoppingToken);
}
