using System.Linq.Expressions;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Defines the base repository contract for aggregate and entity persistence.
/// </summary>
/// <typeparam name="T">The entity type managed by the repository.</typeparam>
public interface IRepository<T>
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The strongly typed entity identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching entity, or <c>null</c> when no entity exists for the given identifier.</returns>
    Task<T?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its identifier without enabling change tracking.
    /// </summary>
    /// <param name="id">The strongly typed entity identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching detached entity, or <c>null</c> when no entity exists for the given identifier.</returns>
    Task<T?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities, optionally including related navigation properties.
    /// </summary>
    /// <param name="includes">Navigation properties to eagerly include.</param>
    /// <returns>The matching entity collection.</returns>
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets all entities, optionally including related navigation properties, while honoring cancellation.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="includes">Navigation properties to eagerly include.</param>
    /// <returns>The matching entity collection.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Adds a new entity to the current unit of work.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The tracked entity instance.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Marks an entity as modified in the current unit of work.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The tracked entity instance.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The strongly typed entity identifier.</param>
    /// <returns><c>true</c> when the entity existed and was marked for deletion; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(ValueObject id);
}
