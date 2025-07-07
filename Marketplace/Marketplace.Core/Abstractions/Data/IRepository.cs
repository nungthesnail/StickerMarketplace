using System.Linq.Expressions;

namespace Marketplace.Core.Abstractions.Data;

public interface IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    Task AddAsync(TEntity entity, CancellationToken stoppingToken = default);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken stoppingToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken stoppingToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);

    Task<IEnumerable<TEntity>> GetByAsync(
        Expression<Func<TEntity, bool>> predicate,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default);
    
    Task<IEnumerable<TEntity>> GetByAsync<TOrderProperty>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TOrderProperty>> orderBy,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default);

    Task<IEnumerable<TAggregation>> GetByGroupsAsync<TGroupKey, TAggregation>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TGroupKey>> keySelector,
        Expression<Func<IGrouping<TGroupKey, TEntity>, TAggregation>> groupSelector,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default);
    
    Task UpdateByAsync<TProperty>(
        Expression<Func<TEntity, bool>> predicate,
        Func<TEntity, TProperty> propertySelector,
        Func<TEntity, TProperty> valueSelector,
        CancellationToken stoppingToken = default);
    
    Task DeleteByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken stoppingToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken stoppingToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken stoppingToken = default);
}
