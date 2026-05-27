using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;

/// <summary>Validates the <see cref="AddBlobContainerCommand"/> before it is handled.</summary>
public sealed class AddBlobContainerCommandValidator : AbstractValidator<AddBlobContainerCommand>
{
    /// <summary>Initializes validation rules for adding a blob container.</summary>
    public AddBlobContainerCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.PublicAccess)
            .NotNull().WithMessage("PublicAccess is required.");
    }
}
