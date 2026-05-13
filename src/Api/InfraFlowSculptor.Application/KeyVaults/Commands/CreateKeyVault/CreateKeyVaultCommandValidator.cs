using FluentValidation;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

/// <summary>Validates the <see cref="CreateKeyVaultCommand"/> before it is handled.</summary>
public sealed class CreateKeyVaultCommandValidator : AbstractValidator<CreateKeyVaultCommand>
{
    /// <summary>Initializes validation rules for creating a Key Vault.</summary>
    public CreateKeyVaultCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}