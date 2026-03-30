namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Coordinates the persistence of all changes tracked within a single request scope.
/// The <see cref="UnitOfWorkBehavior{TRequest,TResponse}"/> calls <see cref="SaveChangesAsync"/>
/// once after the handler succeeds, ensuring atomic writes.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes tracked by the current DbContext.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
