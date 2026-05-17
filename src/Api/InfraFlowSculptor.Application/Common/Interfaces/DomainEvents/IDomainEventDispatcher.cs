using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces.DomainEvents;

/// <summary>
/// Dispatches domain events to in-process handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches the provided domain events.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
