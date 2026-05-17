using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;

/// <summary>Validates the <see cref="CreateStorageAccountCommand"/> before it is handled.</summary>
public sealed class CreateStorageAccountCommandValidator : CreateResourceCommandValidator<CreateStorageAccountCommand>
{
    /// <summary>Initializes validation rules for creating a Storage Account.</summary>
    public CreateStorageAccountCommandValidator()
    {
        RuleFor(x => x.Kind)
            .NotEmpty().WithMessage("Kind is required.")
            .Must(value => Enum.TryParse<StorageAccountKind.Kind>(value, out _))
            .WithMessage("Kind must be a valid storage account kind.");

        RuleFor(x => x.AccessTier)
            .NotEmpty().WithMessage("AccessTier is required.")
            .Must(value => Enum.TryParse<StorageAccessTier.Tier>(value, out _))
            .WithMessage("AccessTier must be a valid storage access tier.");

        RuleFor(x => x.MinimumTlsVersion)
            .NotEmpty().WithMessage("MinimumTlsVersion is required.")
            .Must(value => Enum.TryParse<StorageAccountTlsVersion.Version>(value, out _))
            .WithMessage("MinimumTlsVersion must be a valid TLS version.");
    }
}
