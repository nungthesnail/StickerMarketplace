namespace Marketplace.Core.Abstractions.Data;

public interface IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    Task AddAsync(TEntity entity, CancellationToken stoppingToken);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken stoppingToken);
    IAsyncEnumerable<TEntity> GetAllAsync(CancellationToken stoppingToken);
    void Update(TEntity entity);
    void Delete(TEntity entity);

    Task<IEnumerable<TEntity>> GetByAsync(
        Func<TEntity, bool> predicate,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default);
    
    Task<IEnumerable<TEntity>> GetByAsync<TOrderProperty>(
        Func<TEntity, bool> predicate,
        Func<TEntity, TOrderProperty> orderBy,
        IEnumerable<string>? joins = null,
        CancellationToken stoppingToken = default);
    
    Task UpdateByAsync<TProperty>(
        Func<TEntity, bool> predicate,
        Func<TEntity, TProperty> propertySelector,
        Func<TEntity, TProperty> valueSelector,
        CancellationToken stoppingToken);
    
    Task DeleteByAsync(Func<TEntity, bool> predicate, CancellationToken stoppingToken);
}
