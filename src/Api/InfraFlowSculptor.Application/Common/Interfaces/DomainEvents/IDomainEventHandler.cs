using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces.DomainEvents;

/// <summary>
/// Handles a specific domain event in-process.
/// </summary>
/// <typeparam name="TDomainEvent">The concrete domain event type.</typeparam>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the provided domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}