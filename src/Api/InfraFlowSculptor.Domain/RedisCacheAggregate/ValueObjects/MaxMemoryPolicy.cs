using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

public class MaxMemoryPolicy(MaxMemoryPolicy.Policy value) : EnumValueObject<MaxMemoryPolicy.Policy>(value)
{
    public enum Policy
    {
        NoEviction,
        AllKeysLru,
        VolatileLru,
        AllKeysRandom,
        VolatileRandom,
        VolatileTtl,
        AllKeysLfu,
        VolatileLfu
    }
}
