using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;

/// <summary>
/// Validates the <see cref="CreateContainerRegistryCommand"/> before it is handled.
/// </summary>
public sealed class CreateContainerRegistryCommandValidator : AbstractValidator<CreateContainerRegistryCommand>
{
    /// <summary>Initializes validation rules for creating a Container Registry.</summary>
    public CreateContainerRegistryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
