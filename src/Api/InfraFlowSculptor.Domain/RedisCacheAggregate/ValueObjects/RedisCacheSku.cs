using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

public class RedisCacheSku(RedisCacheSku.RedisCacheSkuEnum value) : EnumValueObject<RedisCacheSku.RedisCacheSkuEnum>(value)
{
    public enum RedisCacheSkuEnum
    {
        Basic,
        Standard,
        Premium
    }
}
