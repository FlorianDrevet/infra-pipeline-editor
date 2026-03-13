using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

public class MaxMemoryPolicy(MaxMemoryPolicy.MaxMemoryPolicyEnum value) : EnumValueObject<MaxMemoryPolicy.MaxMemoryPolicyEnum>(value)
{
    public enum MaxMemoryPolicyEnum
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
