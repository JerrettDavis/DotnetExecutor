namespace Executor.Infrastructure.Persistence.Common;

public interface IRepo<TKey, TEntity>
{
    Task<IEnumerable<TEntity>> GetAsync(CancellationToken cancellationToken = default);
    Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TKey key, TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey key, CancellationToken cancellationToken = default);
}