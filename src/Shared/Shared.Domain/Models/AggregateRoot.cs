using BicepGenerator.Domain.Common.Models;

namespace Shared.Domain.Domain.Models;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot() : base()
    {
    }
    
    public static implicit operator TId(AggregateRoot<TId> aggregateRoot) => aggregateRoot.Id;
}