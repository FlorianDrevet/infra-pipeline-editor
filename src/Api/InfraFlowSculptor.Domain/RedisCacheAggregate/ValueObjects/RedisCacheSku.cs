using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

public class RedisCacheSku(RedisCacheSku.Sku value) : EnumValueObject<RedisCacheSku.Sku>(value)
{
    public enum Sku
    {
        Basic,
        Standard,
        Premium
    }
}
