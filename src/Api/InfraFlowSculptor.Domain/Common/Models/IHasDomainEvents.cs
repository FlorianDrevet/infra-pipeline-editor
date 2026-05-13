namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Exposes domain events raised by an aggregate root.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events raised by the aggregate.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears the domain events raised by the aggregate.
    /// </summary>
    void ClearDomainEvents();
}