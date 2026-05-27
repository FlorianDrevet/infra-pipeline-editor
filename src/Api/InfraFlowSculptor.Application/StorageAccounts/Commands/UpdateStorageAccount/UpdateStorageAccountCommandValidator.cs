using FluentValidation;
using InfraFlowSculptor.Application.StorageAccounts.Common;

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

        this.AddStorageAccountRules();
    }
}
