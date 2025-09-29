namespace InfraFlowSculptor.Domain.Common.Models;

public abstract class Id<TId> : ValueObject
{
    public Guid Value { get; protected set; }

    protected Id(Guid value)
    {
        Value = value;
    }

    public static TId CreateUnique()
    {
        return (TId)Activator.CreateInstance(typeof(TId), Guid.NewGuid())!;
    }
    
    public static TId Create(Guid value)
    {
        return (TId)Activator.CreateInstance(typeof(TId), value)!;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}