using FluentValidation;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;

/// <summary>Validates the <see cref="DeleteKeyVaultCommand"/>.</summary>
public sealed class DeleteKeyVaultCommandValidator : AbstractValidator<DeleteKeyVaultCommand>
{
    public DeleteKeyVaultCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
