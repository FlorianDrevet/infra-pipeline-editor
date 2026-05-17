using FluentValidation;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Common;

/// <summary>
/// Registers validation rules shared between <c>CreateStorageAccountCommandValidator</c>
/// and <c>UpdateStorageAccountCommandValidator</c>.
/// </summary>
public static class StorageAccountValidationRules
{
    /// <summary>
    /// Adds the common Storage Account validation rules (Kind, AccessTier, MinimumTlsVersion)
    /// to the validator.
    /// </summary>
    /// <typeparam name="T">A command type implementing <see cref="IStorageAccountCommandProperties"/>.</typeparam>
    public static void AddStorageAccountRules<T>(this AbstractValidator<T> validator)
        where T : IStorageAccountCommandProperties
    {
        validator.RuleFor(x => x.Kind)
            .NotEmpty().WithMessage("Kind is required.")
            .Must(value => Enum.TryParse<StorageAccountKind.Kind>(value, out _))
            .WithMessage("Kind must be a valid storage account kind.");

        validator.RuleFor(x => x.AccessTier)
            .NotEmpty().WithMessage("AccessTier is required.")
            .Must(value => Enum.TryParse<StorageAccessTier.Tier>(value, out _))
            .WithMessage("AccessTier must be a valid storage access tier.");

        validator.RuleFor(x => x.MinimumTlsVersion)
            .NotEmpty().WithMessage("MinimumTlsVersion is required.")
            .Must(value => Enum.TryParse<StorageAccountTlsVersion.Version>(value, out _))
            .WithMessage("MinimumTlsVersion must be a valid TLS version.");
    }
}
