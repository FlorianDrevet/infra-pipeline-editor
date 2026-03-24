using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;

/// <summary>
/// Validates the <see cref="CreateContainerAppCommand"/> before it is handled.
/// </summary>
public sealed class CreateContainerAppCommandValidator : AbstractValidator<CreateContainerAppCommand>
{
    /// <summary>Initializes validation rules for creating a Container App.</summary>
    public CreateContainerAppCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.ContainerAppEnvironmentId)
            .NotEmpty().WithMessage("ContainerAppEnvironmentId is required.");
    }
}
