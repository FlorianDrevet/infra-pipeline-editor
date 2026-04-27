using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.DeleteContainerRegistry;

/// <summary>Validates the <see cref="DeleteContainerRegistryCommand"/>.</summary>
public sealed class DeleteContainerRegistryCommandValidator : AbstractValidator<DeleteContainerRegistryCommand>
{
    public DeleteContainerRegistryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
