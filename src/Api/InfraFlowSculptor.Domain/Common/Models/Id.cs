namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Base class for strongly-typed identifiers wrapping a <see cref="Guid"/>.
/// Provides factory methods <see cref="CreateUnique"/> and <see cref="Create"/>.
/// </summary>
/// <typeparam name="TId">The concrete identifier type.</typeparam>
public abstract class Id<TId> : ValueObject
{
    /// <summary>Gets the underlying <see cref="Guid"/> value.</summary>
    public Guid Value { get; protected set; }

    /// <summary>Initializes a new identifier with the given <see cref="Guid"/>.</summary>
    protected Id(Guid value)
    {
        Value = value;
    }

    /// <summary>Creates a new identifier with a randomly generated <see cref="Guid"/>.</summary>
    public static TId CreateUnique()
    {
        return (TId)Activator.CreateInstance(typeof(TId), Guid.NewGuid())!;
    }

    /// <summary>Creates an identifier wrapping an existing <see cref="Guid"/>.</summary>
    public static TId Create(Guid value)
    {
        return (TId)Activator.CreateInstance(typeof(TId), value)!;
    }

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
