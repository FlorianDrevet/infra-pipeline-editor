using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.RedisCaches.Requests;

public class CreateRedisCacheRequest : RedisCacheRequestBase, IValidatableObject
{
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    [Required, Range(0, 6)]
    // Basic/Standard supports C0-C6 (0-6); Premium supports P1-P4 (1-4). Cross-SKU validation is enforced via object-level validation.
    public required int Capacity { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Let existing EnumValidation attribute handle invalid SKU values.
        if (!Enum.TryParse<RedisCacheSku.Sku>(Sku, ignoreCase: false, out var parsedSku))
        {
            yield break;
        }

        switch (parsedSku)
        {
            case RedisCacheSku.Sku.Premium:
                if (Capacity < 1 || Capacity > 4)
                {
                    yield return new ValidationResult(
                        "For Premium SKU, capacity must be between 1 and 4.",
                        new[] { nameof(Capacity) });
                }
                break;

            case RedisCacheSku.Sku.Basic:
            case RedisCacheSku.Sku.Standard:
                if (Capacity < 0 || Capacity > 6)
                {
                    yield return new ValidationResult(
                        "For Basic and Standard SKUs, capacity must be between 0 and 6.",
                        new[] { nameof(Capacity) });
                }
                break;

            default:
                // For any other SKU types, rely on separate business rules or validations.
                break;
        }
    }
}
