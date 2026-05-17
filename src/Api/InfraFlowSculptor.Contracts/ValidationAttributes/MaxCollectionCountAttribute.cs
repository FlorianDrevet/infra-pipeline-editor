using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxCollectionCountAttribute(int maxCount) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (GetCount(value) <= maxCount)
        {
            return ValidationResult.Success;
        }

        var memberName = validationContext.MemberName ?? validationContext.DisplayName;
        var errorMessage = $"The {validationContext.DisplayName} field must contain {maxCount} items or fewer.";
        return memberName is null
            ? new ValidationResult(errorMessage)
            : new ValidationResult(errorMessage, [memberName]);
    }

    private static int GetCount(object value)
    {
        if (value is ICollection collection)
        {
            return collection.Count;
        }

        var count = 0;
        foreach (var _ in (IEnumerable)value)
        {
            count++;
        }

        return count;
    }
}
