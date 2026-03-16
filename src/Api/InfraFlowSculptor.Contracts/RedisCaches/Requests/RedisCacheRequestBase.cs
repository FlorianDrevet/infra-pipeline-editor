using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.RedisCaches.Requests;

public abstract class RedisCacheRequestBase
{
    [Required]
    public required string Name { get; init; }

    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    [Required, EnumValidation(typeof(RedisCacheSku.Sku))]
    public required string Sku { get; init; }

    [Required, RedisVersionValidation]
    public required int RedisVersion { get; init; }

    [Required]
    public required bool EnableNonSslPort { get; init; }

    [Required, EnumValidation(typeof(TlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    [Required, EnumValidation(typeof(MaxMemoryPolicy.Policy))]
    public required string MaxMemoryPolicy { get; init; }
}
