using FluentValidation;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;

/// <summary>Validates the <see cref="UpdateStorageAccountCommand"/> before it is handled.</summary>
public sealed class UpdateStorageAccountCommandValidator : AbstractValidator<UpdateStorageAccountCommand>
{
    /// <summary>Initializes validation rules for updating a Storage Account.</summary>
    public UpdateStorageAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

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
