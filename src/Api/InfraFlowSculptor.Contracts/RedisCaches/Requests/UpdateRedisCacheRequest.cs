using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.RedisCaches.Requests;

public class UpdateRedisCacheRequest
{
    [Required]
    public required string Name { get; init; }

    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    [Required, EnumValidation(typeof(RedisCacheSku.Sku))]
    public required string Sku { get; init; }

    [Required, Range(0, 6)]
    // Basic/Standard supports C0-C6 (0-6); Premium supports P1-P4 (1-4). Cross-SKU validation is enforced at the domain level.
    public required int Capacity { get; init; }

    [Required, RedisVersionValidation]
    public required int RedisVersion { get; init; }

    [Required]
    public required bool EnableNonSslPort { get; init; }

    [Required, EnumValidation(typeof(TlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    [Required, EnumValidation(typeof(MaxMemoryPolicy.Policy))]
    public required string MaxMemoryPolicy { get; init; }
}

public sealed class RedisVersionValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int intValue && (intValue == 4 || intValue == 6))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("RedisVersion must be either 4 or 6.");
    }
}
