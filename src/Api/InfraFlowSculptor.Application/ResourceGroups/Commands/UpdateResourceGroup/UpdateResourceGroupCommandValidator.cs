using FluentValidation;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.UpdateResourceGroup;

/// <summary>
/// Validates the <see cref="UpdateResourceGroupCommand"/> before it is handled.
/// </summary>
public sealed class UpdateResourceGroupCommandValidator : AbstractValidator<UpdateResourceGroupCommand>
{
    private const int ResourceGroupNameMaxLength = 90;

    /// <summary>Initializes validation rules for updating a resource group.</summary>
    public UpdateResourceGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Name.Value)
            .MaximumLength(ResourceGroupNameMaxLength)
            .WithMessage($"Name must not exceed {ResourceGroupNameMaxLength} characters.")
            .OverridePropertyName(nameof(UpdateResourceGroupCommand.Name))
            .When(x => x.Name is not null);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}
