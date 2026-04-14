using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

/// <summary>Redis maxmemory eviction policy controlling behavior when memory limit is reached.</summary>
public class MaxMemoryPolicy(MaxMemoryPolicy.Policy value) : EnumValueObject<MaxMemoryPolicy.Policy>(value)
{
    /// <summary>Supported eviction policies.</summary>
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
