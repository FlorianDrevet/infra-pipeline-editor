using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

public class Environment : ValueObject
{
    public enum EnvironmentEnum
    {
        Development,
        Staging,
        Production,
        Unknown
    }

    public EnvironmentEnum Value { get; protected set; }

    public Environment()
    {
    }

    public Environment(EnvironmentEnum value)
    {
        this.Value = value;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}