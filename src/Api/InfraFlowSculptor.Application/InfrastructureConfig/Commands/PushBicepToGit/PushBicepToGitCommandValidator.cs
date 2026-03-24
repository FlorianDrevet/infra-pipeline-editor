using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBicepToGit;

/// <summary>Validates the <see cref="PushBicepToGitCommand"/> before it is handled.</summary>
public sealed class PushBicepToGitCommandValidator
    : AbstractValidator<PushBicepToGitCommand>
{
    public PushBicepToGitCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("BranchName is required.")
            .MaximumLength(200).WithMessage("BranchName must not exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9/_.\-]+$").WithMessage("BranchName contains invalid characters.");

        RuleFor(x => x.CommitMessage)
            .NotEmpty().WithMessage("CommitMessage is required.")
            .MaximumLength(500).WithMessage("CommitMessage must not exceed 500 characters.");
    }
}
