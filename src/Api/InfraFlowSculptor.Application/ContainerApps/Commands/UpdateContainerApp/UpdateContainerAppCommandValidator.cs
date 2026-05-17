using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;

/// <summary>Validates the <see cref="UpdateContainerAppCommand"/> before it is handled.</summary>
public sealed class UpdateContainerAppCommandValidator : AbstractValidator<UpdateContainerAppCommand>
{
    /// <summary>Initializes validation rules for updating a Container App.</summary>
    public UpdateContainerAppCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.ContainerAppEnvironmentId)
            .NotEmpty().WithMessage("ContainerAppEnvironmentId is required.");

        RuleFor(x => x.ContainerRegistryId!.Value)
            .NotEmpty().WithMessage("ContainerRegistryId must not be empty when provided.")
            .When(x => x.ContainerRegistryId.HasValue);
    }
}
