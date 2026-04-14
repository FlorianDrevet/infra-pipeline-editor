using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;

/// <summary>
/// Validates the <see cref="UpdateContainerRegistryCommand"/> before it is handled.
/// </summary>
public sealed class UpdateContainerRegistryCommandValidator : AbstractValidator<UpdateContainerRegistryCommand>
{
    /// <summary>Initializes validation rules for updating a Container Registry.</summary>
    public UpdateContainerRegistryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
