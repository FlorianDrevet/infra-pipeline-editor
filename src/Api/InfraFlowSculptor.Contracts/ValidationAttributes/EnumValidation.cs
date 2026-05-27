using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EnumValidation : ValidationAttribute // NOSONAR S3376 — short name without Attribute suffix is intentional
{
    private readonly Type _enumType;

    public EnumValidation(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("Type must be an enum", nameof(enumType));
        }
        _enumType = enumType;
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            // Utiliser [Required] pour valider null.
            return true;
        }

        if (value is string rawValue)
        {
            return Array.Exists(
                Enum.GetNames(_enumType),
                enumName => string.Equals(enumName, rawValue, StringComparison.OrdinalIgnoreCase));
        }

        return Enum.IsDefined(_enumType, value);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return !IsValid(value) ? new ValidationResult($"The value '{value}' is not valid for enum type '{_enumType.Name}'.") : ValidationResult.Success;
    }
}
