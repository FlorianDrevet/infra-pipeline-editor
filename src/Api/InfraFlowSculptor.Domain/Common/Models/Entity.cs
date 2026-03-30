namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Base class for DDD entities. Identity-based equality: two entities are equal
/// when they have the same <typeparamref name="TId"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>Gets the unique identifier for this entity.</summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>Initializes a new entity with the given identifier.</summary>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>EF Core constructor.</summary>
    protected Entity()
    {
    }

    /// <inheritdoc />
    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    /// <summary>Determines whether two entities have the same identity.</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>Determines whether two entities have different identities.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
