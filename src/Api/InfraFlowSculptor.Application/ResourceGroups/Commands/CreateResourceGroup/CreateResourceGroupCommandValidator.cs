using FluentValidation;

namespace InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;

/// <summary>
/// Validates the <see cref="CreateResourceGroupCommand"/> before it is handled.
/// </summary>
public sealed class CreateResourceGroupCommandValidator : AbstractValidator<CreateResourceGroupCommand>
{
    private const int ResourceGroupNameMaxLength = 90;

    /// <summary>Initializes validation rules for creating a resource group.</summary>
    public CreateResourceGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Name.Value)
            .MaximumLength(ResourceGroupNameMaxLength)
            .WithMessage($"Name must not exceed {ResourceGroupNameMaxLength} characters.")
            .OverridePropertyName(nameof(CreateResourceGroupCommand.Name))
            .When(x => x.Name is not null);

        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}