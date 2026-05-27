using FluentValidation;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;

/// <summary>Validates the <see cref="UpdateKeyVaultCommand"/> before it is handled.</summary>
public sealed class UpdateKeyVaultCommandValidator : AbstractValidator<UpdateKeyVaultCommand>
{
    /// <summary>Initializes validation rules for updating a Key Vault.</summary>
    public UpdateKeyVaultCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}
