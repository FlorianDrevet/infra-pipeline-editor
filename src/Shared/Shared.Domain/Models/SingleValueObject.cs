using Shared.Domain.Domain.Models;

namespace Shared.Domain.Models;

public abstract class SingleValueObject<T>: ValueObject
{
    public T Value { get; set; } = default!;
    
    protected SingleValueObject() { }

    protected SingleValueObject(T value)
    {
        Value = value;
    }
    
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}