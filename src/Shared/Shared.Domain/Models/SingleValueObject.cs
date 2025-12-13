using Shared.Domain.Domain.Models;

namespace Shared.Domain.Models;

public class SingleValueObject<T>(T value): ValueObject
{
    public T Value { get; set; } = value;
    
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}