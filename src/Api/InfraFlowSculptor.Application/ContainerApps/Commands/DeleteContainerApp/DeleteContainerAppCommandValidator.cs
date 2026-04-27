using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.DeleteContainerApp;

/// <summary>Validates the <see cref="DeleteContainerAppCommand"/>.</summary>
public sealed class DeleteContainerAppCommandValidator : AbstractValidator<DeleteContainerAppCommand>
{
    public DeleteContainerAppCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
