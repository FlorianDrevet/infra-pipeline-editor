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

        RuleForEach(x => x.EnvironmentSettings)
            .ChildRules(env =>
            {
                env.RuleFor(e => e.ReadinessProbePort)
                    .InclusiveBetween(1, 65535)
                    .When(e => e.ReadinessProbePort.HasValue)
                    .WithMessage("ReadinessProbePort must be between 1 and 65535.");

                env.RuleFor(e => e.ReadinessProbePath)
                    .Must(p => p!.StartsWith('/'))
                    .When(e => e.ReadinessProbePath is not null)
                    .WithMessage("ReadinessProbePath must start with '/'.");

                env.RuleFor(e => e.LivenessProbePort)
                    .InclusiveBetween(1, 65535)
                    .When(e => e.LivenessProbePort.HasValue)
                    .WithMessage("LivenessProbePort must be between 1 and 65535.");

                env.RuleFor(e => e.LivenessProbePath)
                    .Must(p => p!.StartsWith('/'))
                    .When(e => e.LivenessProbePath is not null)
                    .WithMessage("LivenessProbePath must start with '/'.");

                env.RuleFor(e => e.StartupProbePort)
                    .InclusiveBetween(1, 65535)
                    .When(e => e.StartupProbePort.HasValue)
                    .WithMessage("StartupProbePort must be between 1 and 65535.");

                env.RuleFor(e => e.StartupProbePath)
                    .Must(p => p!.StartsWith('/'))
                    .When(e => e.StartupProbePath is not null)
                    .WithMessage("StartupProbePath must start with '/'.");
            })
            .When(x => x.EnvironmentSettings is not null);
    }
}
