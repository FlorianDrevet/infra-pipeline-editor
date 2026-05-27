using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class GuidValidation : ValidationAttribute // NOSONAR S3376 — short name without Attribute suffix is intentional
{
    public GuidValidation()
    {
        ErrorMessage = "The {0} field must be a valid non-empty GUID.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var isValid = value switch
        {
            null => true,
            string stringValue when string.IsNullOrWhiteSpace(stringValue) => true,
            string stringValue => Guid.TryParse(stringValue, out var parsedGuid) && parsedGuid != Guid.Empty,
            Guid guid => guid != Guid.Empty,
            _ => false
        };

        return isValid
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}
