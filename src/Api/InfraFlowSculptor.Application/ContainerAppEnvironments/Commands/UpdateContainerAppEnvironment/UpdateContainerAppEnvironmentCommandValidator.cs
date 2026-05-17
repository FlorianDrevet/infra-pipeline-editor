using FluentValidation;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;

/// <summary>Validates the <see cref="UpdateContainerAppEnvironmentCommand"/> before it is handled.</summary>
public sealed class UpdateContainerAppEnvironmentCommandValidator : AbstractValidator<UpdateContainerAppEnvironmentCommand>
{
    /// <summary>Initializes validation rules for updating a Container App Environment.</summary>
    public UpdateContainerAppEnvironmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.LogAnalyticsWorkspaceId!.Value)
            .NotEmpty().WithMessage("LogAnalyticsWorkspaceId must not be empty when provided.")
            .When(x => x.LogAnalyticsWorkspaceId.HasValue);
    }
}
