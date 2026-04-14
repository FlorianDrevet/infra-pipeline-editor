namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Base class for DDD aggregate roots. An aggregate root is the entry point
/// for all operations on the aggregate and enforces its invariants.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>Initializes a new aggregate root with the given identifier.</summary>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>EF Core constructor.</summary>
    protected AggregateRoot() : base()
    {
    }

    /// <summary>Implicit conversion to the aggregate root's identifier.</summary>
    public static implicit operator TId(AggregateRoot<TId> aggregateRoot) => aggregateRoot.Id;
}
