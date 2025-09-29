using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

public sealed class SecretUserEntityId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static SecretUserEntityId CreateUnique()
    {
        return new SecretUserEntityId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static SecretUserEntityId Create(Guid value)
    {
        return new SecretUserEntityId(value);
    }
}