using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

public class SecretSource : ValueObject
{
    public enum SecretSourceEnum
    {
        AzureResource,
        User,
    }

    public SecretSourceEnum Value { get; protected set; }

    public SecretSource()
    {
    }

    public SecretSource(SecretSourceEnum value)
    {
        this.Value = value;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}