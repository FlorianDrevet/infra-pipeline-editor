using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public class StorageAccessTier(StorageAccessTier.Tier value) : EnumValueObject<StorageAccessTier.Tier>(value)
{
    public enum Tier
    {
        Hot,
        Cool
    }
}
