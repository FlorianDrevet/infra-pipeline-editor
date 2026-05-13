using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;

/// <summary>Validates the <see cref="RemoveProjectEnvironmentCommand"/> before it is handled.</summary>
public sealed class RemoveProjectEnvironmentCommandValidator : AbstractValidator<RemoveProjectEnvironmentCommand>
{
    /// <summary>Initializes validation rules for removing a project environment.</summary>
    public RemoveProjectEnvironmentCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.EnvironmentId.Value)
            .NotEmpty().WithMessage("EnvironmentId is required.");
    }
}