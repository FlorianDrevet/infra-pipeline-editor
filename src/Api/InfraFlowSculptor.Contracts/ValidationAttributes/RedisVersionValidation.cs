using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RedisVersionValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int intValue && (intValue == 4 || intValue == 6))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            "RedisVersion must be either 4 or 6.",
            new[] { validationContext.MemberName ?? "RedisVersion" });
    }
}
