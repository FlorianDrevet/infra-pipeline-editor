using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectGitConfig;

/// <summary>Validates the <see cref="RemoveProjectGitConfigCommand"/>.</summary>
public sealed class RemoveProjectGitConfigCommandValidator
    : AbstractValidator<RemoveProjectGitConfigCommand>
{
    public RemoveProjectGitConfigCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
