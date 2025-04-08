using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

public sealed class SoftDelete(bool enabled = true, int retentionInDays = 30) : ValueObject
{
    public bool Enabled { get; protected set; } = enabled;
    public int RetentionInDays { get; protected set; } = retentionInDays;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Enabled;
        yield return RetentionInDays;
    }
}