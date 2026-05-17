using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;

/// <summary>Validates the <see cref="UpdateBlobContainerPublicAccessCommand"/> before it is handled.</summary>
public sealed class UpdateBlobContainerPublicAccessCommandValidator : AbstractValidator<UpdateBlobContainerPublicAccessCommand>
{
    /// <summary>Initializes validation rules for updating blob container public access.</summary>
    public UpdateBlobContainerPublicAccessCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.ContainerId)
            .NotEmpty().WithMessage("ContainerId is required.");

        RuleFor(x => x.PublicAccess)
            .NotNull().WithMessage("PublicAccess is required.");
    }
}
