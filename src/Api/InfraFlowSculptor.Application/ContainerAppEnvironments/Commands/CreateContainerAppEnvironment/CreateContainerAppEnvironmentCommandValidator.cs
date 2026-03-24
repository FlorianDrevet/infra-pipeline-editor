using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;

/// <summary>
/// Validates the <see cref="CreateContainerAppEnvironmentCommand"/> before it is handled.
/// </summary>
public sealed class CreateContainerAppEnvironmentCommandValidator : AbstractValidator<CreateContainerAppEnvironmentCommand>
{
    /// <summary>Initializes validation rules for creating a Container App Environment.</summary>
    public CreateContainerAppEnvironmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
