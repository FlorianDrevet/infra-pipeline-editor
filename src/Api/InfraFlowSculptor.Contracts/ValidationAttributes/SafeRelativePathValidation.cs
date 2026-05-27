using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.Common;

namespace InfraFlowSculptor.Contracts.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SafeRelativePathValidation : ValidationAttribute // NOSONAR S3376 — short name without Attribute suffix is intentional
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeRelativePathValidation"/> class.
    /// </summary>
    public SafeRelativePathValidation()
    {
        ErrorMessage = "The {0} field must be a safe relative repository path.";
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var isValid = value switch
        {
            null => true,
            string stringValue => SafeRelativePath.TryNormalize(stringValue, out _),
            _ => false
        };

        return isValid
            ? ValidationResult.Success
            : new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                validationContext.MemberName is null ? null : [validationContext.MemberName]);
    }
}