using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

/// <summary>Pricing tier for an Azure Cache for Redis instance.</summary>
public class RedisCacheSku(RedisCacheSku.Sku value) : EnumValueObject<RedisCacheSku.Sku>(value)
{
    /// <summary>Available Redis Cache pricing tiers.</summary>
    public enum Sku
    {
        Basic,
        Standard,
        Premium
    }
}
