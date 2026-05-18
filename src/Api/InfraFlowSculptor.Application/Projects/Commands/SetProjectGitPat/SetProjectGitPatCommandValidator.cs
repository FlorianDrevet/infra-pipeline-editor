using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectGitPat;

/// <summary>
/// Validates the <see cref="SetProjectGitPatCommand"/> before it is handled.
/// </summary>
public sealed class SetProjectGitPatCommandValidator : AbstractValidator<SetProjectGitPatCommand>
{
    /// <summary>
    /// Initializes the validator.
    /// </summary>
    public SetProjectGitPatCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.RepositoryId)
            .NotEmpty().WithMessage("RepositoryId is required.");

        RuleFor(x => x.PersonalAccessToken)
            .NotEmpty().WithMessage("PersonalAccessToken is required.");
    }
}