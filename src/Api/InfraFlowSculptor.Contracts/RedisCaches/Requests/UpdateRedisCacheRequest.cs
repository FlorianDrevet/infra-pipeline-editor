using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.RedisCaches.Requests;

public class UpdateRedisCacheRequest : RedisCacheRequestBase
{
    [Required, Range(0, 6)]
    // Basic/Standard supports C0-C6 (0-6); Premium supports P1-P4 (1-4). Cross-SKU validation is enforced here via CustomValidation.
    [CustomValidation(typeof(UpdateRedisCacheRequest), nameof(ValidateCapacityForSku))]
    public required int Capacity { get; init; }

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

        if (!Enum.TryParse<RedisCacheSku.Sku>(instance.Sku, ignoreCase: false, out var skuEnum))
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
