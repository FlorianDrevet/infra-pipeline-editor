namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Base class for value objects wrapping a single primitive value.
/// Provides implicit conversion to <typeparamref name="T"/> and structural equality.
/// </summary>
/// <typeparam name="T">The underlying primitive type.</typeparam>
public abstract class SingleValueObject<T> : ValueObject
{
    /// <summary>Gets the underlying primitive value.</summary>
    public T Value { get; private set; } = default!;

    /// <summary>EF Core / serialization constructor.</summary>
    protected SingleValueObject() { }

    /// <summary>Initializes a new instance with the given value.</summary>
    protected SingleValueObject(T value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>Implicit conversion to the underlying primitive type.</summary>
    public static implicit operator T(SingleValueObject<T> valueObject) => valueObject.Value;
}
