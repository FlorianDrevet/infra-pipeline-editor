using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;

/// <summary>Validates the <see cref="DeleteContainerAppEnvironmentCommand"/>.</summary>
public sealed class DeleteContainerAppEnvironmentCommandValidator : AbstractValidator<DeleteContainerAppEnvironmentCommand>
{
    public DeleteContainerAppEnvironmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
