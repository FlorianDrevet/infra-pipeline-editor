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
    // Basic/Standard supports C0-C6 (0-6); Premium supports P1-P4 (1-4). Cross-SKU validation is enforced here via CustomValidation.
    [CustomValidation(typeof(UpdateRedisCacheRequest), nameof(ValidateCapacityForSku))]
    public required int Capacity { get; init; }

    [Required, RedisVersionValidation]
    public required int RedisVersion { get; init; }

    [Required]
    public required bool EnableNonSslPort { get; init; }

    [Required, EnumValidation(typeof(TlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    [Required, EnumValidation(typeof(MaxMemoryPolicy.Policy))]
    public required string MaxMemoryPolicy { get; init; }

    public static ValidationResult? ValidateCapacityForSku(object? value, ValidationContext validationContext)
    {
        if (validationContext?.ObjectInstance is not UpdateRedisCacheRequest instance)
        {
            return ValidationResult.Success;
        }

        // Let other validators handle missing or out-of-range values.
        if (value is not int capacity)
        {
            return ValidationResult.Success;
        }

        // If SKU is not provided, defer to other validators.
        if (string.IsNullOrWhiteSpace(instance.Sku))
        {
            return ValidationResult.Success;
        }

        if (!Enum.TryParse<RedisCacheSku.Sku>(instance.Sku, ignoreCase: true, out var skuEnum))
        {
            // EnumValidation attribute on Sku will surface a clearer error.
            return ValidationResult.Success;
        }

        var isPremium = skuEnum == RedisCacheSku.Sku.Premium;

        if (isPremium)
        {
            if (capacity < 1 || capacity > 4)
            {
                return new ValidationResult(
                    "For Premium Redis caches, Capacity must be between 1 and 4.",
                    new[] { nameof(Capacity) });
            }
        }
        else
        {
            if (capacity < 0 || capacity > 6)
            {
                return new ValidationResult(
                    "For Basic/Standard Redis caches, Capacity must be between 0 and 6.",
                    new[] { nameof(Capacity) });
            }
        }

        return ValidationResult.Success;
    }
}

[AttributeUsage(AttributeTargets.Property)]
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
