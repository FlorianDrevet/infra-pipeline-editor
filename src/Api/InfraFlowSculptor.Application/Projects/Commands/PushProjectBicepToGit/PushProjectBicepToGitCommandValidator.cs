using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;

/// <summary>Validates the <see cref="PushProjectBicepToGitCommand"/> before it is handled.</summary>
public sealed class PushProjectBicepToGitCommandValidator
    : AbstractValidator<PushProjectBicepToGitCommand>
{
    public PushProjectBicepToGitCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("BranchName is required.")
            .MaximumLength(200).WithMessage("BranchName must not exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9/_.\-]+$").WithMessage("BranchName contains invalid characters.");

        RuleFor(x => x.CommitMessage)
            .NotEmpty().WithMessage("CommitMessage is required.")
            .MaximumLength(500).WithMessage("CommitMessage must not exceed 500 characters.");
    }
}
