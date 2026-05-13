namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Base class for DDD aggregate roots. An aggregate root is the entry point
/// for all operations on the aggregate and enforces its invariants.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Initializes a new aggregate root with the given identifier.</summary>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>EF Core constructor.</summary>
    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Gets the domain events raised by the aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to register.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears the domain events raised by the aggregate.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>Implicit conversion to the aggregate root's identifier.</summary>
    public static implicit operator TId(AggregateRoot<TId> aggregateRoot) => aggregateRoot.Id;
}
