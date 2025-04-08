using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

public sealed class SecretAzureResourceEntityId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static SecretAzureResourceEntityId CreateUnique()
    {
        return new SecretAzureResourceEntityId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static SecretAzureResourceEntityId Create(Guid value)
    {
        return new SecretAzureResourceEntityId(value);
    }
}