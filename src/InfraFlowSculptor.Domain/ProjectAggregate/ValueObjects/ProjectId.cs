using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

public sealed class ProjectId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static ProjectId CreateUnique()
    {
        return new ProjectId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static ProjectId Create(Guid value)
    {
        return new ProjectId(value);
    }
}