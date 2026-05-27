using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;

/// <summary>Validates the <see cref="RemoveBlobContainerCommand"/> before it is handled.</summary>
public sealed class RemoveBlobContainerCommandValidator : AbstractValidator<RemoveBlobContainerCommand>
{
    /// <summary>Initializes validation rules for removing a blob container.</summary>
    public RemoveBlobContainerCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.ContainerId)
            .NotEmpty().WithMessage("ContainerId is required.");
    }
}
