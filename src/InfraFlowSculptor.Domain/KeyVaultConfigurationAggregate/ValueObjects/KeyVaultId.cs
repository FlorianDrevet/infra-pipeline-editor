using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

public sealed class KeyVaultId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static KeyVaultId CreateUnique()
    {
        return new KeyVaultId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static KeyVaultId Create(Guid value)
    {
        return new KeyVaultId(value);
    }
}