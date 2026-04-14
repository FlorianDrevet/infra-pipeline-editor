using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Default access tier for an Azure Storage Account (Hot, Cool, or Premium).</summary>
public class StorageAccessTier(StorageAccessTier.Tier value) : EnumValueObject<StorageAccessTier.Tier>(value)
{
    /// <summary>Available access tiers.</summary>
    public enum Tier
    {
        Hot,
        Cool,
        Premium
    }
}
