using System.Linq.Expressions;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by its identifier, applying a caller-supplied query transformation
    /// (typically <c>.Include()</c> / <c>.ThenInclude()</c>) before executing the query.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="queryBuilder">
    /// A function that receives the base <see cref="IQueryable{T}"/> and returns a transformed
    /// queryable — for example <c>q => q.Include(e => e.Navigation)</c>.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    Task<T?> GetByIdAsync(ValueObject id, Func<IQueryable<T>, IQueryable<T>> queryBuilder, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(ValueObject id);
}
